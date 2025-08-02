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
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        public EmployeeController(IConfiguration configuration, UserManager<IdentityUser> userManager,
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
        [HttpPost("registerEmployee")]
        public async Task<IActionResult> Register([FromForm] EmployeesRigesterDto dto)
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
                return Conflict(new
                {
                    status = 409,
                    message = "Email already registered."
                });
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

                if (!await _roleManager.RoleExistsAsync("Receptionist"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Receptionist"));
                }

                await _userManager.AddToRoleAsync(user, "Receptionist");

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


        [HttpPost("verify-email-Employee")]
        public async Task<IActionResult> VerifyEmail([FromForm] VerifyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.VerificationCode))
                return BadRequest(new { status = 400, message = "Email and verification code are required." });

            var employee = await _context.Employeess.FirstOrDefaultAsync(d => d.Email == dto.Email);

            if (employee == null)
                return NotFound(new { status = 404, message = "User not found." });

            if (employee.IsVerified == true)
                return BadRequest(new { status = 400, message = "Account already verified." });

            if (employee.VerificationCode != dto.VerificationCode)
                return BadRequest(new { status = 400, message = "Invalid verification code." });

            if (employee.CodeExpiresAt < DateTime.UtcNow)
            {
                var newVerificationCode = GenerateVerificationCode();
                employee.VerificationCode = newVerificationCode;
                employee.CodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

                await _context.SaveChangesAsync();

                await SendVerificationEmail(employee.Email, newVerificationCode);

                return BadRequest(new { status = 400, message = "Verification code expired. A new code has been sent to your email." });
            }

            employee.IsVerified = true;

            employee.VerificationCode = string.Empty;
            employee.CodeExpiresAt = null;

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

        [HttpPost("forgot-password-employee")]
        public async Task<IActionResult> ForgotPassword([FromForm] string email)
        {
            var employee = await _context.Employeess.FirstOrDefaultAsync(d => d.Email == email);

            if (employee == null)
                return NotFound(new { status = 404, message = "Email not found." });

            var resetCode = GenerateVerificationCode();
            employee.VerificationCode = resetCode;
            employee.CodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            await SendVerificationEmail(employee.Email, resetCode);

            return Ok(new { status = 200, message = "Verification code sent to your email." });
        }






        [HttpPost("reset-password-employee")]
        public async Task<IActionResult> ResetPasswordEmployee(
     [FromHeader(Name = "Email")] string email,
     [FromForm] string VerificationCode,
     [FromForm] string NewPassword)
        {
            if (!Regex.IsMatch(NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10}$"))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Password must be exactly 10 characters and include at least one uppercase letter, one lowercase letter, one digit, and one special character."
                });
            }

            var employee = await _context.Employeess.FirstOrDefaultAsync(e => e.Email == email);

            if (employee == null)
                return NotFound(new { status = 404, message = "Email not found." });

            if (employee.VerificationCode != VerificationCode)
                return BadRequest(new { status = 400, message = "Invalid verification code." });

            if (employee.CodeExpiresAt < DateTime.UtcNow)
                return BadRequest(new { status = 400, message = "Verification code expired." });

            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            employee.VerificationCode = null;
            employee.CodeExpiresAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Password has been reset successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] EmployeeLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    status = 400,
                    message = "Validation failed.",
                    errors = errors
                });
            }

            var identityUser = await _userManager.FindByEmailAsync(dto.Email);
            if (identityUser == null)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, dto.Password);
            if (!passwordValid)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            var employee = await _context.Employeess.SingleOrDefaultAsync(e => e.Email == dto.Email);
            if (employee == null)
            {
                return Unauthorized(new { status = 401, message = "Employee record not found." });
            }

            if (!employee.IsVerified)
            {
                return Unauthorized(new { status = 401, message = "Account is not verified. Please check your email." });
            }

            var result = await GenerateJwtToken(employee);

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token = result.token,
                expiration = result.expiration,
                refreshToken = result.refreshToken,
                employee = new
                {
                    employee.Id,
                    employee.First_Name,
                    employee.Last_Name,
                    employee.Email,
                    employee.PhoneNumber
                }
            });
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        private async Task<(string token, DateTime expiration, string refreshToken)> GenerateJwtToken(Employees employee)
        {
            var identityUser = await _userManager.FindByEmailAsync(employee.Email);

            if (identityUser == null)
            {
                throw new ArgumentException("User not found for the given email.");
            }

            var roles = await _userManager.GetRolesAsync(identityUser);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, employee.Email),
        new Claim("employeeId", employee.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, employee.Email)
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

            var refreshToken = GenerateRefreshToken();

            employee.RefreshToken = refreshToken;
            employee.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _context.Employeess.Update(employee);
            await _context.SaveChangesAsync();

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration, refreshToken);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromForm] string token, [FromForm] string refreshToken)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Token and refresh token are required." });

            var principal = GetPrincipalFromExpiredToken(token);
            var email = principal?.Identity?.Name;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "Invalid token or user not found." });

            var employee = await _context.Employeess.FirstOrDefaultAsync(x => x.Email == email);

            if (employee == null)
                return Unauthorized(new { message = "User not found." });

            if (employee.RefreshToken != refreshToken)
                return Unauthorized(new { message = "Refresh token does not match." });

            if (employee.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized(new { message = "Refresh token has expired." });

            var newTokenData = await GenerateJwtToken(employee);
            var newRefreshToken = GenerateRefreshToken();

            employee.RefreshToken = newRefreshToken;
            employee.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = newTokenData.token,
                expiration = newTokenData.expiration,
                refreshToken = newRefreshToken
            });
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }



        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !long.TryParse(employeeIdClaim, out long employeeId))
            {
                return Unauthorized(new { status = 401, message = "Invalid user token." });
            }

            var employee = await _context.Employeess.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound(new { status = 404, message = "Employee not found." });
            }

            var identityUser = await _userManager.FindByEmailAsync(employee.Email);
            if (identityUser != null)
            {
                var deleteResult = await _userManager.DeleteAsync(identityUser);
                if (!deleteResult.Succeeded)
                {
                    return StatusCode(500, new
                    {
                        status = 500,
                        message = "Failed to delete identity user.",
                        errors = deleteResult.Errors
                    });
                }
            }

            _context.Employeess.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Employee account and identity user deleted successfully." });
        }


        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                status = 200,
                message = "Logged out successfully."
            });
        }

        [HttpPost("registerSecretary")]
        public async Task<IActionResult> RegisterSecretary([FromForm] EmployeesRigesterDto dto)
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

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (string.IsNullOrWhiteSpace(dto.Email) || !emailRegex.IsMatch(dto.Email) || !dto.Email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid email format. Example of valid email: name@example.com"
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

                if (!await _roleManager.RoleExistsAsync("Secretary"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Secretary"));
                }

                await _userManager.AddToRoleAsync(user, "Secretary");

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


        [HttpPost("loginSecretary")]
        public async Task<IActionResult> LoginSecretary([FromForm] EmployeeLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    status = 400,
                    message = "Validation failed.",
                    errors = errors
                });
            }

            var employee = await _context.Employeess.SingleOrDefaultAsync(d => d.Email == dto.Email);
            if (employee == null)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { status = 401, message = "Invalid email or password." });
            }

            if (!employee.IsVerified)
            {
                return Unauthorized(new { status = 401, message = "Account is not verified. Please check your email." });
            }

            if (string.IsNullOrEmpty(employee.IdentityUserId))
            {
                return Unauthorized(new { status = 401, message = "User identity not found." });
            }

            var identityUser = await _userManager.FindByIdAsync(employee.IdentityUserId);
            if (identityUser == null)
            {
                return Unauthorized(new { status = 401, message = "User identity not found." });
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            if (!roles.Contains("Secretary"))
            {
                return Forbid();
            }


            var result = await GenerateJwtToken(employee);

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token = result.token,
                expiration = result.expiration,
                refreshToken = result.refreshToken,
                employee = new
                {
                    employee.Id,
                    employee.First_Name,
                    employee.Last_Name,
                    employee.Email,
                    employee.PhoneNumber
                }
            });
        }


        [Authorize(Roles = "Driver")]
        [HttpPost("editprofile-employee")]
        public async Task<IActionResult> EditProfileEmployee([FromForm] EmployeeProfileUpdateDto dto)
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

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long employeeId))
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });

            var employee = await _context.Employeess.FindAsync(employeeId);
            if (employee == null)
                return NotFound(new { status = 404, message = "Employee not found." });

            if (!string.IsNullOrWhiteSpace(dto.First_Name))
                employee.First_Name = dto.First_Name;

            if (!string.IsNullOrWhiteSpace(dto.Middel_name))
                employee.Middel_name = dto.Middel_name;

            if (!string.IsNullOrWhiteSpace(dto.Last_Name))
                employee.Last_Name = dto.Last_Name;

            if (!string.IsNullOrWhiteSpace(dto.Residence))
                employee.Residence = dto.Residence;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                employee.PhoneNumber = dto.PhoneNumber;

            if (dto.Age.HasValue)
                employee.Age = dto.Age.Value;

            if (dto.ID_Number.HasValue)
                employee.ID_Number = dto.ID_Number.Value;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                employee.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Employee profile updated successfully.",
                data = new
                {
                    employee.Id,
                    full_name = $"{employee.First_Name} {employee.Middel_name} {employee.Last_Name}".Trim(),
                    employee.PhoneNumber,
                    age = employee.Age.HasValue ? employee.Age.Value.ToString("yyyy-MM-dd") : null,
                    employee.ID_Number,
                    employee.Residence,
                    employee.Email
                }
            });
        }

        [Authorize(Roles = "Receptionist,Secretary")]
        [HttpPost("editprofile-receptionist-secretary")]
        public async Task<IActionResult> EditProfileReceptionistSecretary([FromForm] EmployeeProfileUpdateDto dto)
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

            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !long.TryParse(employeeIdClaim, out long employeeId))
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });

            var employee = await _context.Employeess.FindAsync(employeeId);
            if (employee == null)
                return NotFound(new { status = 404, message = "Employee not found." });

            if (!string.IsNullOrWhiteSpace(dto.First_Name))
                employee.First_Name = dto.First_Name;

            if (!string.IsNullOrWhiteSpace(dto.Middel_name))
                employee.Middel_name = dto.Middel_name;

            if (!string.IsNullOrWhiteSpace(dto.Last_Name))
                employee.Last_Name = dto.Last_Name;

            if (!string.IsNullOrWhiteSpace(dto.Residence))
                employee.Residence = dto.Residence;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                employee.PhoneNumber = dto.PhoneNumber;

            if (dto.Age.HasValue)
                employee.Age = dto.Age.Value;

            if (dto.ID_Number.HasValue)
                employee.ID_Number = dto.ID_Number.Value;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                employee.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Employee profile updated successfully.",
                data = new
                {
                    employee.Id,
                    full_name = $"{employee.First_Name} {employee.Middel_name} {employee.Last_Name}".Trim(),
                    employee.PhoneNumber,
                    age = employee.Age.HasValue ? employee.Age.Value.ToString("yyyy-MM-dd") : null,
                    employee.ID_Number,
                    employee.Residence,
                    employee.Email
                }
            });
        }


        [Authorize(Roles = "Receptionist,Secretary")]
        [HttpGet("profile-receptionist-secretary")]
        public async Task<IActionResult> GetProfileReceptionistSecretary()
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !long.TryParse(employeeIdClaim, out long employeeId))
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });

            var employee = await _context.Employeess.FindAsync(employeeId);
            if (employee == null)
                return NotFound(new { status = 404, message = "Employee not found." });

            return Ok(new
            {
                status = 200,
                message = "Employee profile fetched successfully.",
                data = new
                {
                    employee.Id,
                    employee.First_Name,
                    employee.Middel_name,
                    employee.Last_Name,
                    employee.PhoneNumber,
                    age = employee.Age.HasValue ? employee.Age.Value.ToString("yyyy-MM-dd") : null,
                    employee.ID_Number,
                    employee.Residence
                }
            });
        }

        [Authorize(Roles = "Driver")]
        [HttpGet("profile-driver")]
        public async Task<IActionResult> GetProfileDriver()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long employeeId))
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });

            var employee = await _context.Employeess.FindAsync(employeeId);
            if (employee == null)
                return NotFound(new { status = 404, message = "Employee not found." });

            return Ok(new
            {
                status = 200,
                message = "Employee profile fetched successfully.",
                data = new
                {
                    full_name = $"{employee.First_Name} {employee.Middel_name} {employee.Last_Name}".Trim(),
                    employee.PhoneNumber,
                    age = employee.Age.HasValue ? employee.Age.Value.ToString("yyyy-MM-dd") : null,
                    employee.ID_Number,
                    employee.Residence,
                    employee.Email
                }
            });
        }






    }



}


