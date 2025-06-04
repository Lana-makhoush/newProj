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

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context; 
        public DoctorsController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] DoctorRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.First_Name) ||
                string.IsNullOrWhiteSpace(dto.Last_Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { status = 400, message = "All fields are required." });
            }

            if (!new EmailAddressAttribute().IsValid(dto.Email))
            {
                return BadRequest(new { status = 400, message = "Invalid email format." });
            }

            if (!dto.PhoneNumber.All(char.IsDigit) || dto.PhoneNumber.Length < 8)
            {
                return BadRequest(new { status = 400, message = "Invalid phone number format." });
            }

            if (dto.Password.Length < 6)
            {
                return BadRequest(new { status = 400, message = "Password must be at least 6 characters." });
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

            try
            {
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> ResetPassword([FromForm] string email, [FromForm] string verificationCode, [FromForm] string newPassword)
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

            if (!doctor.IsVerified == true)
            {
                return Unauthorized(new { status = 401, message = "Account is not verified. Please check your email." });
            }

            var token = GenerateJwtToken(doctor);

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token = token,
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

        private string GenerateJwtToken(Doctors doctor)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, doctor.Email),
            new Claim("doctorId", doctor.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var doctorIdClaim = User.FindFirst("doctorId")?.Value;

            if (string.IsNullOrEmpty(doctorIdClaim))
            {
                return Unauthorized(new { status = 401, message = "User not authenticated." });
            }

            // تحويل المعرف إلى int
            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                return BadRequest(new { status = 400, message = "Invalid user ID format." });
            }

            // البحث عن الطبيب في قاعدة البيانات
            var doctor = await _context.Doctorss.FindAsync(doctorId);

            if (doctor == null)
            {
                return NotFound(new { status = 404, message = "Doctor not found." });
            }

            _context.Doctorss.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Doctor account deleted successfully." });
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

        [Authorize]
        [Authorize]
        [HttpPost("update-doctor-profile")]
        public async Task<IActionResult> UpdateDoctorProfile([FromForm] DoctorProfileUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int doctorId))
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

    }
}
