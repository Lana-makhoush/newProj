using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System;
using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using l_hospital_mang.DTOs;
using l_hospital_mang.DTOs.l_hospital_mang.DTOs;
//using l_hospital_mang.Migrations;
using Employees = l_hospital_mang.Data.Models.Employees;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DriverController(AppDbContext context, IConfiguration configuration,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpPost("registerDriver")]
        public async Task<IActionResult> RegisterDriver([FromForm] EmployeesRigesterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

                return BadRequest(new
                {
                    status = 400,
                    message = "Validation failed.",
                    errors = errors
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                !dto.Email.Contains("@") ||
                !dto.Email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid email format. Email must contain '@' and end with '.com' (e.g., example@gmail.com)."
                });
            }

            var exists = await _context.Employeess.AnyAsync(d => d.Email == dto.Email);
            if (exists)
            {
                return Conflict(new { status = 409, message = "Email already registered." });
            }

            var verificationCode = GenerateVerificationCode();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var empolyee = new Employees
            {
                First_Name = dto.First_Name,
                Middel_name = dto.Middel_name,
                Last_Name = dto.Last_Name,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                PasswordHash = passwordHash,
                IsVerified = false,
                VerificationCode = verificationCode,
                CodeExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Age = dto.Age,
                ID_Number = dto.ID_Number
            };

            _context.Employeess.Add(empolyee);

            try
            {
                await _context.SaveChangesAsync();

                var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    _context.Employeess.Remove(empolyee);
                    await _context.SaveChangesAsync();
                    return BadRequest(new
                    {
                        status = 400,
                        message = "Failed to create user identity.",
                        errors = result.Errors
                    });
                }

                empolyee.IdentityUserId = user.Id;
                await _context.SaveChangesAsync();

                if (!await _roleManager.RoleExistsAsync("Driver"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Driver"));
                }

                await _userManager.AddToRoleAsync(user, "Driver");

                await SendVerificationEmail(empolyee.Email, verificationCode);
            }
            catch (Exception ex)
            {
                _context.Employeess.Remove(empolyee);
                await _context.SaveChangesAsync();

                return StatusCode(500, new
                {
                    status = 500,
                    message = "Failed to send verification email.",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Registration successful. Please check your email to verify your account."
            });
        }

        private string GenerateVerificationCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        private async Task SendVerificationEmail(string toEmail, string code)
        {
            var smtpServer = "smtp.gmail.com";
            var port = 587;
            var senderEmail = "lanamakhoush19@gmail.com";
            var senderPassword = "wmwj jknk udgz znzp";

            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress("Hospital App", senderEmail));
            message.To.Add(MimeKit.MailboxAddress.Parse(toEmail));
            message.Subject = "Email Verification Code";

            message.Body = new MimeKit.TextPart("plain")
            {
                Text = $"Your verification code is: {code}\nThis code is valid for 15 minutes."
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, senderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] DriverLoginDto dto)
        {
            var employee = await _context.Employeess.SingleOrDefaultAsync(e => e.Email == dto.Email);
            if (employee == null || !BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
                return Unauthorized(new { status = 401, message = "Invalid email or password." });

            var (token, expiration, refreshToken) = GenerateJwtToken(employee.Id.ToString(), employee.Email, "Driver");

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token,
                expiration,
                refreshToken,
                employee = new { employee.Id, employee.First_Name, employee.Last_Name, employee.Email, employee.PhoneNumber }
            });
        }

        [Authorize(Roles = "Driver")]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var driverIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(driverIdClaim) || !long.TryParse(driverIdClaim, out long driverId))
                return Unauthorized(new { status = 401, message = "Unauthorized or missing token." });

            var employee = await _context.Employeess.FindAsync(driverId);
            if (employee == null)
                return NotFound(new { status = 404, message = "Driver not found." });

            var identityUser = await _userManager.FindByEmailAsync(employee.Email);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);

            _context.Employeess.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Account deleted successfully." });
        }

       
        private (string token, DateTime expiration, string refreshToken) GenerateJwtToken(string userId, string email, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim("userId", userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            return (jwtToken, expiration, refreshToken);
        }
    }
}
