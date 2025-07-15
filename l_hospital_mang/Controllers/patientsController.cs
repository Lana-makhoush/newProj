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
using Microsoft.Extensions.Hosting;
using l_hospital_mang.DTOs.l_hospital_mang.DTOs;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;


        public PatientsController(IConfiguration configuration, UserManager<IdentityUser> userManager,
         RoleManager<IdentityRole> roleManager,
         SignInManager<IdentityUser> signInManager,
         AppDbContext context
             , IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _environment = environment;
        }

        [Authorize(Roles = "Secretary")]

        [HttpPost("AddPatient")]
        public IActionResult AddPatient([FromForm] a_patient_dto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = new patient
            {
                First_Name = dto.First_Name,
                Middel_name = dto.Middel_name,
                Last_Name = dto.Last_Name,
                Age = dto.Age,
                Residence = dto.Residence,
                ID_Number = dto.ID_Number,
                PhoneNumber = dto.PhoneNumber,
                ImagePath = null
            };

            try
            {
                _context.Patients.Add(patient);
                _context.SaveChanges();

                return Ok(new
                {
                    id = patient.Id,
                    first_Name = patient.First_Name,
                    middel_name = patient.Middel_name,
                    last_Name = patient.Last_Name,
                    age = patient.Age?.ToString("yyyy-MM-dd"),
                    residence = patient.Residence,
                    iD_Number = patient.ID_Number,
                    phoneNumber = patient.PhoneNumber
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"DbUpdateException: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }
        [Authorize(Roles = "Secretary")]

        [HttpPut("UpdatePatient/{id}")]
        public IActionResult UpdatePatient(long id, [FromForm] UpdatePatientDTO dto)
        {
            var patient = _context.Patients.Find(id);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            if (!string.IsNullOrWhiteSpace(dto.First_Name))
                patient.First_Name = dto.First_Name;

            if (!string.IsNullOrWhiteSpace(dto.Middel_name))
                patient.Middel_name = dto.Middel_name;

            if (!string.IsNullOrWhiteSpace(dto.Last_Name))
                patient.Last_Name = dto.Last_Name;

            if (dto.Age.HasValue)
                patient.Age = dto.Age;

            if (!string.IsNullOrWhiteSpace(dto.Residence))
                patient.Residence = dto.Residence;

            if (!string.IsNullOrWhiteSpace(dto.ID_Number))
                patient.ID_Number = dto.ID_Number;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                patient.PhoneNumber = dto.PhoneNumber;

            try
            {
                _context.SaveChanges();

                return Ok(new
                {
                    id = patient.Id,
                    first_Name = patient.First_Name,
                    middel_name = patient.Middel_name,
                    last_Name = patient.Last_Name,
                    age = patient.Age?.ToString("yyyy-MM-dd"),
                    residence = patient.Residence,
                    iD_Number = patient.ID_Number,
                    phoneNumber = patient.PhoneNumber
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"DbUpdateException: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }
        [Authorize(Roles = "Secretary")]

        [HttpDelete("DeletePatient/{id}")]
        public IActionResult DeletePatient(long id)
        {
            var patient = _context.Patients.Find(id);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            try
            {
                _context.Patients.Remove(patient);
                _context.SaveChanges();

                return Ok(new
                {
                    message = "Patient deleted successfully",
                   
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"DbUpdateException: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterPatient([FromForm] PatientRegisterDto dto)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(dto.First_Name) || !dto.First_Name.All(c => char.IsLetter(c) || c == ' '))
                return BadRequest(new { status = 400, message = "First name is required and must contain only letters." });

            if (string.IsNullOrWhiteSpace(dto.Middel_name) || !dto.Middel_name.All(c => char.IsLetter(c) || c == ' '))
                return BadRequest(new { status = 400, message = "Middle name is required and must contain only letters." });

            if (string.IsNullOrWhiteSpace(dto.Last_Name) || !dto.Last_Name.All(c => char.IsLetter(c) || c == ' '))
                return BadRequest(new { status = 400, message = "Last name is required and must contain only letters." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { status = 400, message = "Email is required." });

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest(new { status = 400, message = "Phone number is required." });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { status = 400, message = "Password is required." });

            var exists = await _context.Patients.AnyAsync(p => p.Email == dto.Email);
            if (exists)
                return Conflict(new { status = 409, message = "Email already registered." });

            var verificationCode = GenerateVerificationCode();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            try
            {
                var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                    return BadRequest(new { status = 400, message = "Failed to create user identity.", errors = result.Errors });

                if (!await _roleManager.RoleExistsAsync("Patient"))
                    await _roleManager.CreateAsync(new IdentityRole("Patient"));

                await _userManager.AddToRoleAsync(user, "Patient");

                var patient = new patient
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
                    IdentityUserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                await SendVerificationEmail(patient.Email, verificationCode);

                return Ok(new { status = 200, message = "Registration successful. Please check your email to verify your account." });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += " | Inner Exception: " + ex.InnerException.Message;

                return StatusCode(500, new { status = 500, message = "An error occurred during registration.", error = errorMessage });
            }
        }


        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyPatientEmail([FromForm] VerifyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.VerificationCode))
                return BadRequest(new { status = 400, message = "Email and verification code are required." });

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == dto.Email);

            if (patient == null)
                return NotFound(new { status = 404, message = "Patient not found." });

            if (patient.IsVerified == true)
                return BadRequest(new { status = 400, message = "Account already verified." });

            if (patient.VerificationCode != dto.VerificationCode)
                return BadRequest(new { status = 400, message = "Invalid verification code." });

            if (patient.CodeExpiresAt < DateTime.UtcNow)
            {
                var newVerificationCode = GenerateVerificationCode();
                patient.VerificationCode = newVerificationCode;
                patient.CodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

                await _context.SaveChangesAsync();

                await SendVerificationEmail(patient.Email, newVerificationCode);

                return BadRequest(new { status = 400, message = "Verification code expired. A new code has been sent to your email." });
            }

            patient.IsVerified = true;

            patient.VerificationCode = null;
            patient.CodeExpiresAt = null;

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
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == email);

            if (patient == null)
                return NotFound(new { status = 404, message = "Email not found." });

            var resetCode = GenerateVerificationCode();
            patient.VerificationCode = resetCode;
            patient.CodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            await SendVerificationEmail(patient.Email, resetCode);

            return Ok(new { status = 200, message = "Verification code sent to your email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return BadRequest(new { status = 400, message = error ?? "Invalid input." });
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == dto.Email);

            if (patient == null)
                return NotFound(new { status = 404, message = "Email not found." });

            if (patient.VerificationCode != dto.VerificationCode)
                return BadRequest(new { status = 400, message = "Invalid verification code." });

            if (patient.CodeExpiresAt < DateTime.UtcNow)
                return BadRequest(new { status = 400, message = "Verification code expired." });

            patient.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            patient.VerificationCode = null;
            patient.CodeExpiresAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Password has been reset successfully." });
        }




        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] PatientLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value.Errors.First().ErrorMessage
                    );

                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Validation failed.",
                    errors = errors
                });
            }

            var patient = await _context.Patients.SingleOrDefaultAsync(p => p.Email == dto.Email);
            if (patient == null)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, patient.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            if ((bool)!patient.IsVerified)
            {
                return Unauthorized(new { status = 401, message = "Account is not verified. Please check your email." });
            }

            // ⬇️ إنشاء التوكن و refresh token
            var (token, expiration, refreshToken) = GenerateJwtToken(
                patient.Id.ToString(),
                patient.Email,
                "Patient"
            );

            // ⬇️ تخزين refresh token وتاريخ انتهاء صلاحيته
            patient.RefreshToken = refreshToken;
            patient.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // أو المدة التي تراها مناسبة
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token = token,
                expiration = expiration,
                refreshToken = refreshToken,
                patient = new
                {
                    patient.Id,
                    patient.First_Name,
                    patient.Last_Name,
                    patient.Email,
                    patient.PhoneNumber
                }
            });
        }



        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var patientIdClaim = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(patientIdClaim) || !long.TryParse(patientIdClaim, out long patientId))
            {
                return Unauthorized(new { status = 401, message = "Unauthorized or missing token." });
            }

            var patient = await _context.Patients
                .Include(p => p.AmbulanceRequests) 
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return NotFound(new { status = 404, message = "Patient not found." });
            }

            if (patient.AmbulanceRequests != null && patient.AmbulanceRequests.Any())
            {
                _context.AmbulanceRequests.RemoveRange(patient.AmbulanceRequests);
            }

            _context.Patients.Remove(patient);

            var identityUser = await _userManager.FindByEmailAsync(patient.Email);
            if (identityUser != null)
            {
                var identityResult = await _userManager.DeleteAsync(identityUser);
                if (!identityResult.Succeeded)
                {
                    return StatusCode(500, new { status = 500, message = "Failed to delete user identity." });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Account and all related data deleted successfully." });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                status = 200,
                message = "Patient logged out successfully. Please delete the token on the client side."
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

        [Authorize(Roles = "Patient")]
        [HttpPut("update-patient-profile")]
        public async Task<IActionResult> UpdatePatientProfile([FromForm] UpdatePatientProfileDto dto)
        {
            var patientIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(patientIdClaim) || !long.TryParse(patientIdClaim, out long patientId))
            {
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound(new { status = 404, message = "Patient not found." });
            }

            patient.Age = dto.Age;
            patient.Residence = dto.Residence;
            patient.ID_Number = dto.ID_Number;
            patient.PhoneNumber = dto.PhoneNumber;

            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "Images", "Patients");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }
                patient.ImagePath = $"/Images/Patients/{fileName}";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Patient profile updated successfully.",
                patient = new
                {
                    patient.Id,
                    patient.First_Name,
                    patient.Middel_name,
                    patient.Last_Name,
                    Age = patient.Age?.ToString("yyyy-MM-dd"),
                    patient.Residence,
                    patient.ID_Number,
                    patient.PhoneNumber,
                    patient.Email,
                    patient.ImagePath,
                    patient.IsVerified,
                    CreatedAt = patient.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                }
            });
        }



        [HttpPost("refresh-token-patient")]
        public async Task<IActionResult> RefreshTokenForPatient([FromForm] TokenRequestDto tokenRequest)
        {
            if (tokenRequest == null || string.IsNullOrEmpty(tokenRequest.Token) || string.IsNullOrEmpty(tokenRequest.RefreshToken))
            {
                return BadRequest(new { status = 400, message = "Token and refresh token are required." });
            }

            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipalFromExpiredToken(tokenRequest.Token);
            }
            catch
            {
                return BadRequest(new { status = 400, message = "Invalid token." });
            }

            var userId = principal.FindFirstValue("userId");
            var email = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var role = principal.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Token does not contain required claims.",
                    claims = principal.Claims.Select(c => new { c.Type, c.Value })
                });
            }

            if (role != "Patient")
            {
                return Unauthorized(new { status = 401, message = "Not authorized as patient." });
            }

            if (!long.TryParse(userId, out long patientId))
            {
                return BadRequest(new { status = 400, message = "Invalid user ID in token." });
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound(new { status = 404, message = "Patient not found." });
            }

            if (patient.RefreshToken != tokenRequest.RefreshToken || patient.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { status = 400, message = "Invalid or expired refresh token." });
            }

            var (newToken, expiration, newRefreshToken) = GenerateJwtToken(userId, email, role);

            patient.RefreshToken = newRefreshToken;
            patient.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Token refreshed successfully.",
                token = newToken,
                expiration = expiration,
                refreshToken = newRefreshToken,
                role = role
            });
        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }






    }
}
