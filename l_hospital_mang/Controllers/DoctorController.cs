using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System;
using l_hospital_mang.DTOs;
using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        public DoctorsController(IConfiguration configuration, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager,
        AppDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] DoctorRegisterDto dto)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(dto.First_Name) || !dto.First_Name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "First name is required and must contain only letters." });

            if (string.IsNullOrWhiteSpace(dto.Middel_name) || !dto.Middel_name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "Middle name is required and must contain only letters." });

            if (string.IsNullOrWhiteSpace(dto.Last_Name) || !dto.Last_Name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "Last name is required and must contain only letters." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { status = 400, message = "Email is required." });

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest(new { status = 400, message = "Phone number is required." });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { status = 400, message = "Password is required." });

            var exists = await _context.Doctorss.AnyAsync(d => d.Email == dto.Email);
            if (exists)
                return Conflict(new { status = 409, message = "Email already registered." });

            var verificationCode = GenerateVerificationCode();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            try
            {
                var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new { status = 400, message = "Failed to create user identity.", errors = result.Errors });
                }

                if (!await _roleManager.RoleExistsAsync("Doctor"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Doctor"));
                }

                await _userManager.AddToRoleAsync(user, "Doctor");

                var doctor = new Doctors
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
                    IdentityUserId = user.Id
                };

                _context.Doctorss.Add(doctor);
                await _context.SaveChangesAsync();

                await SendVerificationEmail(doctor.Email, verificationCode);

                return Ok(new { status = 200, message = "Registration successful. Please check your email to verify your account." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = "An error occurred during registration.", error = ex.Message });
            }
        }



        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromForm] VerifyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.VerificationCode))
                return BadRequest(new { status = 400, message = "Email and verification code are required." });

            var doctor = await _context.Doctorss.FirstOrDefaultAsync(d => d.Email == dto.Email);

            if (doctor == null)
                return NotFound(new { status = 404, message = "User not found." });

            if (doctor.IsVerified == true)
                return BadRequest(new { status = 400, message = "Account already verified." });

            if (doctor.VerificationCode != dto.VerificationCode)
                return BadRequest(new { status = 400, message = "Invalid verification code." });

            if (doctor.CodeExpiresAt < DateTime.UtcNow)
            {
                var newVerificationCode = GenerateVerificationCode();
                doctor.VerificationCode = newVerificationCode;
                doctor.CodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

                await _context.SaveChangesAsync();

                await SendVerificationEmail(doctor.Email, newVerificationCode);

                return BadRequest(new { status = 400, message = "Verification code expired. A new code has been sent to your email." });
            }

            doctor.IsVerified = true;

            doctor.VerificationCode = null;
            doctor.CodeExpiresAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Email verified successfully." });
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hospital App", senderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Email Verification Code";

            message.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {code}\nThis code is valid for 15 minutes."
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(senderEmail, senderPassword);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromForm] string email)
        {
            var doctor = await _context.Doctorss.FirstOrDefaultAsync(d => d.Email == email);

            if (doctor == null)
                return NotFound(new { status = 404, message = "Email not found." });

            var resetCode = GenerateVerificationCode();
            doctor.VerificationCode = resetCode;
            doctor.CodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            await SendVerificationEmail(doctor.Email, resetCode);

            return Ok(new { status = 200, message = "Verification code sent to your email." });
        }




        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
    [FromHeader(Name = "email")] string email,
    [FromForm] string verificationCode,
    [FromForm] string newPassword)
        {
            var doctor = await _context.Doctorss.FirstOrDefaultAsync(d => d.Email == email);

            if (doctor == null)
                return NotFound(new { status = 404, message = "Email not found." });

            if (doctor.VerificationCode != verificationCode)
                return BadRequest(new { status = 400, message = "Invalid verification code." });

            if (doctor.CodeExpiresAt < DateTime.UtcNow)
                return BadRequest(new { status = 400, message = "Verification code expired." });

            if (newPassword.Length < 6 || !Regex.IsMatch(newPassword, @"^(?=.*[a-zA-Z])(?=.*\d).+$"))
            {
                return BadRequest(new { status = 400, message = "Password must be at least 6 characters long and contain both letters and numbers." });
            }

            doctor.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            doctor.VerificationCode = null;
            doctor.CodeExpiresAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Password has been reset successfully." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] DoctorLoginDto dto)
        {
            var doctor = await _context.Doctorss.SingleOrDefaultAsync(d => d.Email == dto.Email);
            if (doctor == null)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, doctor.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            if ((bool)!doctor.IsVerified)
            {
                return Unauthorized(new { status = 401, message = "Account is not verified. Please check your email." });
            }

            var (token, expiration, refreshToken) = GenerateJwtToken(
                doctor.Id.ToString(),
                doctor.Email,
                "Doctor"
            );

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token = token,
                expiration = expiration,
                refreshToken = refreshToken,
                doctor = new
                {
                    doctor.Id,
                    doctor.First_Name,
                    doctor.Last_Name,
                    doctor.Email,
                    doctor.PhoneNumber
                }
            });
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

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var doctorIdClaim = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(doctorIdClaim) || !long.TryParse(doctorIdClaim, out long doctorId))
            {
                return Unauthorized(new { status = 401, message = "Invalid user token." });
            }

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
            {
                return NotFound(new { status = 404, message = "Doctor not found." });
            }

            _context.Doctorss.Remove(doctor);

            var identityUser = await _userManager.FindByEmailAsync(doctor.Email);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }

            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Account deleted successfully." });
        }


        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                status = 200,
                message = "Doctor logged out successfully. Please delete the token on the client side."
            });
        }


        [Authorize(Roles = "Doctor")]
        [HttpPost("update-doctor-profile")]
        public async Task<IActionResult> UpdateDoctorProfile([FromForm] DoctorProfileUpdateDto dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long doctorId))
            {
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });
            }

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
            {
                return NotFound(new { status = 404, message = "Doctor not found." });
            }

            doctor.Residence = dto.Residence;
            doctor.Overview = dto.Overview;

            if (dto.PdfFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await dto.PdfFile.CopyToAsync(memoryStream);
                    doctor.PdfFile = memoryStream.ToArray();
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Doctor profile updated successfully." });
        }


        [HttpGet("doctors/simple-list")]
        public async Task<ActionResult<IEnumerable<object>>> GetDoctorsSimpleList()
        {
            var doctors = await _context.Doctorss
                .Select(d => new
                {
                    d.Id,
                    FullName = $"{d.First_Name} {d.Middel_name} {d.Last_Name}"
                })
                .ToListAsync();

            return Ok(doctors);
        }
        [HttpPut("surgery-reservations/{reservationId}/assign-doctor/{doctorId}")]
        public async Task<IActionResult> AssignDoctorToReservation(long reservationId, long doctorId)
        {
            var reservation = await _context.surgery_reservationss
                .Include(r => r.Doctor)
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return NotFound(new { message = "Surgery reservation not found." });

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            reservation.DoctorId = doctorId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Doctor assigned to surgery reservation successfully.",
                data = new
                {
                    reservation.Id,
                    reservation.PatientId,
                    patient = reservation.Patient == null ? null : new
                    {
                        reservation.Patient.Id,
                        reservation.Patient.First_Name,
                        reservation.Patient.Middel_name,
                        reservation.Patient.Last_Name,
                        reservation.Patient.PhoneNumber,
                        reservation.Patient.Residence,
                        Age = reservation.Patient.Age.HasValue
                            ? reservation.Patient.Age.Value.ToString("dd/MM/yyyy")
                            : null
                    },
                    reservation.DoctorId,
                    doctor = new
                    {
                        Id = doctor.Id,
                        FullName = $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}"
                    },
                    reservation.SurgeryDate,
                    reservation.SurgeryTime,
                    reservation.SurgeryType,
                    reservation.Price
                }
            });
        }



        [AllowAnonymous]
        [HttpPost("registerManager")]
        public async Task<IActionResult> RegisterManager([FromForm] DoctorRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.First_Name) ||
                string.IsNullOrWhiteSpace(dto.Last_Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { status = 400, message = "All fields are required." });
            }

            var exists = await _context.Doctorss.AnyAsync(d => d.Email == dto.Email);
            if (exists)
            {
                return Conflict(new { status = 409, message = "Email already registered." });
            }

            var verificationCode = GenerateVerificationCode();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var doctor = new Doctors
            {
                First_Name = dto.First_Name,
                Middel_name = dto.Middel_name,
                Last_Name = dto.Last_Name,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                PasswordHash = passwordHash,
                IsVerified = false,
                VerificationCode = verificationCode,
                CodeExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            _context.Doctorss.Add(doctor);
            await _context.SaveChangesAsync();

            try
            {
                var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    _context.Doctorss.Remove(doctor);
                    await _context.SaveChangesAsync();
                    return BadRequest(new { status = 400, message = "Failed to create user identity.", errors = result.Errors });
                }

                doctor.IdentityUserId = user.Id;
                _context.Doctorss.Update(doctor);
                await _context.SaveChangesAsync();

                if (!await _roleManager.RoleExistsAsync("Manager"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Manager"));
                }

                await _userManager.AddToRoleAsync(user, "Manager");
                await SendVerificationEmail(doctor.Email, verificationCode);
            }
            catch (Exception ex)
            {
                _context.Doctorss.Remove(doctor);
                await _context.SaveChangesAsync();

                return StatusCode(500, new { status = 500, message = "Failed to send verification email.", error = ex.Message });
            }

            return Ok(new { status = 200, message = "Registration successful. Please check your email to verify your account." });
        }


        [AllowAnonymous]
        [HttpPost("loginManager")]
        public async Task<IActionResult> LoginManager([FromForm] DoctorLoginDto dto)
        {
            var manager = await _context.Doctorss.SingleOrDefaultAsync(m => m.Email == dto.Email);
            if (manager == null)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, manager.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            if ((bool)!manager.IsVerified)
            {
                return Unauthorized(new { status = 401, message = "Account is not verified. Please check your email." });
            }

            if (string.IsNullOrEmpty(manager.IdentityUserId))
            {
                return Unauthorized(new { status = 401, message = "User identity not found." });
            }

            var identityUser = await _userManager.FindByIdAsync(manager.IdentityUserId);
            if (identityUser == null)
            {
                return Unauthorized(new { status = 401, message = "User identity not found." });
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            if (!roles.Contains("Manager"))
            {
                return Forbid();
            }

            var (token, expiration, refreshToken) = GenerateJwtToken(
                manager.Id.ToString(),
                manager.Email,
                "Manager"
            );

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token = token,
                expiration = expiration,
                refreshToken = refreshToken,
                manager = new
                {
                    manager.Id,
                    manager.First_Name,
                    manager.Last_Name,
                    manager.Email,
                    manager.PhoneNumber
                }
            });
        }



    }
}
