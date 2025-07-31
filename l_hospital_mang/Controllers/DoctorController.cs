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
using System.Security.Cryptography;
using System.Security.Claims;

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
        private readonly IWebHostEnvironment _environment;


        public DoctorsController(IConfiguration configuration, UserManager<IdentityUser> userManager,
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

            if (!dto.Email.Contains('@') || !dto.Email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid email format. Email must contain '@' and end with '.com' (e.g., example@gmail.com)."
                });
            }

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
                    return BadRequest(new
                    {
                        status = 400,
                        message = "Failed to create user identity.",
                        errors = result.Errors
                    });
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

                return Ok(new
                {
                    status = 200,
                    message = "Registration successful. Please check your email to verify your account."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = "An error occurred during registration.",
                    error = ex.Message
                });
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

            if (!Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10}$"))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Password must be exactly 10 characters and include at least one uppercase letter, one lowercase letter, one digit, and one special character."
                });
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

            doctor.RefreshToken = refreshToken;
            doctor.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync(); 

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



        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]
        [HttpPost("update-doctor-profile")]
        public async Task<IActionResult> UpdateDoctorProfile([FromForm] DoctorProfileUpdateDto dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long doctorId))
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { status = 404, message = "Doctor not found." });

            doctor.Residence = dto.Residence;
            doctor.Overview = dto.Overview;

            string pdfBase64 = null;
            string pdfUrl = null;
            string imageUrl = null;

            if (dto.PdfFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await dto.PdfFile.CopyToAsync(memoryStream);
                    doctor.PdfFile = memoryStream.ToArray();
                }

                pdfBase64 = Convert.ToBase64String(doctor.PdfFile);

                var pdfFileName = $"{Guid.NewGuid()}.pdf";
                var pdfRelativePath = Path.Combine("uploads", pdfFileName);
                var pdfAbsolutePath = Path.Combine(_environment.WebRootPath, pdfRelativePath);

                await System.IO.File.WriteAllBytesAsync(pdfAbsolutePath, doctor.PdfFile);

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                pdfUrl = $"{baseUrl}/{pdfRelativePath.Replace("\\", "/")}";
            }

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var doctorFolder = Path.Combine("uploads", "doctor");
                var doctorFolderPath = Path.Combine(_environment.WebRootPath, doctorFolder);

                if (!Directory.Exists(doctorFolderPath))
                    Directory.CreateDirectory(doctorFolderPath);

                var imageFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var imageRelativePath = Path.Combine(doctorFolder, imageFileName);
                var imageAbsolutePath = Path.Combine(_environment.WebRootPath, imageRelativePath);

                using (var stream = new FileStream(imageAbsolutePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                doctor.ImagePath = imageRelativePath.Replace("\\", "/");

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                imageUrl = $"{baseUrl}/{doctor.ImagePath}";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Doctor profile updated successfully.",
                data = new
                {
                    doctorId = doctor.Id,
                    fullName = $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}",
                    email = doctor.Email,
                    phoneNumber = doctor.PhoneNumber,
                    residence = doctor.Residence,
                    imagePath = imageUrl,
                   
                }
            });
        }
        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]
        [HttpPost("edit-doctor-profile")]
        public async Task<IActionResult> EditDoctorProfile([FromForm] edit_dotoctor_profile dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long doctorId))
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { status = 404, message = "Doctor not found." });

            if (!string.IsNullOrWhiteSpace(dto.First_Name))
                doctor.First_Name = dto.First_Name;

            if (!string.IsNullOrWhiteSpace(dto.Middel_name))
                doctor.Middel_name = dto.Middel_name;

            if (!string.IsNullOrWhiteSpace(dto.Last_Name))
                doctor.Last_Name = dto.Last_Name;

            if (!string.IsNullOrWhiteSpace(dto.Residence))
                doctor.Residence = dto.Residence;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                doctor.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Overview))
                doctor.Overview = dto.Overview;

            string pdfUrl = null;
            string imageUrl = null;

            if (dto.PdfFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await dto.PdfFile.CopyToAsync(memoryStream);
                    doctor.PdfFile = memoryStream.ToArray();
                }

                var pdfFileName = $"{Guid.NewGuid()}.pdf";
                var pdfRelativePath = Path.Combine("uploads", pdfFileName);
                var pdfAbsolutePath = Path.Combine(_environment.WebRootPath, pdfRelativePath);

                await System.IO.File.WriteAllBytesAsync(pdfAbsolutePath, doctor.PdfFile);

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                pdfUrl = $"{baseUrl}/{pdfRelativePath.Replace("\\", "/")}";
            }

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var doctorFolder = Path.Combine("uploads", "doctor");
                var doctorFolderPath = Path.Combine(_environment.WebRootPath, doctorFolder);

                if (!Directory.Exists(doctorFolderPath))
                    Directory.CreateDirectory(doctorFolderPath);

                var imageFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var imageRelativePath = Path.Combine(doctorFolder, imageFileName);
                var imageAbsolutePath = Path.Combine(_environment.WebRootPath, imageRelativePath);

                using (var stream = new FileStream(imageAbsolutePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                doctor.ImagePath = imageRelativePath.Replace("\\", "/");

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                imageUrl = $"{baseUrl}/{doctor.ImagePath}";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Doctor profile updated successfully.",
                data = new
                {
                    doctorId = doctor.Id,
                    fullName = $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}",
                    email = doctor.Email,
                    phoneNumber = doctor.PhoneNumber,
                    residence = doctor.Residence,
                    overview = doctor.Overview,
                    imageUrl = imageUrl,
                    pdfUrl = pdfUrl
                }
            });
        }


        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]
        [HttpGet("get-doctor-profile")]
        public async Task<IActionResult> GetDoctorProfile()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long doctorId))
                return Unauthorized(new { status = 401, message = "Invalid or missing token." });

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { status = 404, message = "Doctor not found." });

            string pdfBase64 = null;
            string pdfUrl = null;
            string imageUrl = null;

            if (doctor.PdfFile != null && doctor.PdfFile.Length > 0)
            {
                pdfBase64 = Convert.ToBase64String(doctor.PdfFile);

                var fileName = $"{doctorId}_profile.pdf";
                var relativePath = Path.Combine("uploads", fileName);
                var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

                if (!System.IO.File.Exists(absolutePath))
                {
                    await System.IO.File.WriteAllBytesAsync(absolutePath, doctor.PdfFile);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                pdfUrl = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
            }

            if (!string.IsNullOrEmpty(doctor.ImagePath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                imageUrl = $"{baseUrl}/{doctor.ImagePath}";
            }

            return Ok(new
            {
                status = 200,
                message = "Doctor profile retrieved successfully.",
                doctor = new
                {
                    doctor.Id,
                    fullName = $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}",
                    doctor.Residence,
                    doctor.PhoneNumber,
                    doctor.Email,
                    doctor.Overview,
                    doctor.ClinicId,
                    pdfFileBase64 = pdfBase64,
                    pdfUrl = pdfUrl,
                    imageUrl = imageUrl
                }
            });
        }

        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]

        [HttpPost("AssignClinicToDoctor/{doctorId}/{clinicId}")]
        public async Task<IActionResult> AssignClinicToDoctor(long doctorId, long clinicId)
        {
            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = $"Doctor with ID {doctorId} not found."
                });
            }

            var clinic = await _context.Clinicscss.FindAsync(clinicId);
            if (clinic == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = $"Clinic with ID {clinicId} not found."
                });
            }

            doctor.ClinicId = clinicId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    status = 200,
                    message = "success"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = "An internal error occurred while assigning the clinic.",
                    error = ex.Message
                });
            }
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

       
        [HttpGet("GetDoctorInfo/{doctorId}")]
        public async Task<IActionResult> GetDoctorInfo(long doctorId)
        {
            var doctor = await _context.Doctorss
                .Include(d => d.Clinic)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = $"Doctor not found."
                });
            }

            var fullName = $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}".Trim();

            var result = new
            {
                Id = doctor.Id,                     
                FullName = fullName,
                ClinicName = doctor.Clinic?.Clinic_Name ?? "Not assigned",
                Overview = doctor.Overview ?? "",
                Phone = doctor.PhoneNumber
            };

            return Ok(new
            {
                StatusCode = 200,
                Message = "Doctor information retrieved successfully.",
                Data = result
            });
        }

        [HttpGet("all-doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.Doctorss
                .Select(d => new
                {     id=d.Id,
                    FullName = d.First_Name + " " + d.Middel_name + " " + d.Last_Name,
                    PhoneNumber = d.PhoneNumber,
                    ClinicName = _context.Clinicscss
                                         .Where(c => c.Id == d.ClinicId)
                                         .Select(c => c.Clinic_Name)
                                         .FirstOrDefault(),
                    Overview = d.Overview
                })
                .ToListAsync();

            if (doctors == null || !doctors.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "No doctors available to display."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "List of all doctors retrieved successfully.",
                doctors = doctors
            });
        }
       
        [HttpGet("search-doctor-by-name")]
        public async Task<IActionResult> SearchDoctorByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Search name is required."
                });
            }

            var doctors = await _context.Doctorss
                .Where(d =>
                    (d.First_Name + " " + d.Middel_name + " " + d.Last_Name).Contains(name))
                .Select(d => new
                { id=d.Id,
                    FullName = d.First_Name + " " + d.Middel_name + " " + d.Last_Name,
                    PhoneNumber = d.PhoneNumber,
                    ClinicName = _context.Clinicscss
                                         .Where(c => c.Id == d.ClinicId)
                                         .Select(c => c.Clinic_Name)
                                         .FirstOrDefault(),
                    Overview = d.Overview
                })
                .ToListAsync();

            if (doctors == null || !doctors.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "No doctors found with the given name."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Matching doctors retrieved successfully.",
                doctors = doctors
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

            if (!dto.First_Name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "First name must contain only letters." });

            if (!string.IsNullOrWhiteSpace(dto.Middel_name) && !dto.Middel_name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "Middle name must contain only letters." });

            if (!dto.Last_Name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "Last name must contain only letters." });

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(dto.Email) || !dto.Email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid email format. A valid email should look like: example@example.com"
                });
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

                return StatusCode(500, new
                {
                    status = 500,
                    message = "Failed to send verification email.",
                    error = ex.Message
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Registration successful. Please check your email to verify your account."
            });
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

            manager.RefreshToken = refreshToken;
            manager.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

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


        [HttpGet("search-clinic-by-name")]
        public async Task<IActionResult> SearchClinicByName([FromQuery] string clinicName)
        {
            if (string.IsNullOrWhiteSpace(clinicName))
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Clinic name is required."
                });
            }

            var clinic = await _context.Clinicscss
                .Where(c => c.Clinic_Name.Contains(clinicName))
                .Select(c => new
                {
                    id = c.Id,
                    clinicName = c.Clinic_Name,
                  
                })
                .FirstOrDefaultAsync();

            if (clinic == null)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    message = "Clinic not found."
                });
            }

            return Ok(new
            {
                statusCode = 200,
              
                clinic = clinic
            });
        }
        [HttpPost("refresh-token-doctor")]
        public async Task<IActionResult> RefreshTokenForDoctor([FromForm] TokenRequestDto tokenRequest)
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

            if (role != "Doctor")
            {
                return Unauthorized(new { status = 401, message = "Not authorized as doctor." });
            }

            if (!long.TryParse(userId, out long doctorId))
            {
                return BadRequest(new { status = 400, message = "Invalid user ID in token." });
            }

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
            {
                return NotFound(new { status = 404, message = "Doctor not found." });
            }

            if (doctor.RefreshToken != tokenRequest.RefreshToken || doctor.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { status = 400, message = "Invalid or expired refresh token." });
            }

            var (newToken, expiration, newRefreshToken) = GenerateJwtToken(userId, email, role);

            doctor.RefreshToken = newRefreshToken;
            doctor.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
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
        [HttpPost("refresh-token-manager")]
        public async Task<IActionResult> RefreshTokenForManager([FromForm] TokenRequestDto tokenRequest)
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

            if (role != "Manager")
            {
                return Unauthorized(new { status = 401, message = "Not authorized as manager." });
            }

            if (!long.TryParse(userId, out long managerId))
            {
                return BadRequest(new { status = 400, message = "Invalid user ID in token." });
            }

            var manager = await _context.Doctorss.FindAsync(managerId);
            if (manager == null)
            {
                return NotFound(new { status = 404, message = "Manager not found." });
            }

            if (manager.RefreshToken != tokenRequest.RefreshToken || manager.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { status = 400, message = "Invalid or expired refresh token." });
            }

            var (newToken, expiration, newRefreshToken) = GenerateJwtToken(userId, email, role);

            manager.RefreshToken = newRefreshToken;
            manager.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
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
                ValidAudience = _configuration["Jwt:Audience"],

                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),

                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        [HttpPost("registerLabDoctor")]
        public async Task<IActionResult> RegisterLabDoctor([FromForm] DoctorRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.First_Name) ||
                string.IsNullOrWhiteSpace(dto.Last_Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { status = 400, message = "All fields are required." });
            }

            if (!dto.First_Name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "First name must contain only letters." });

            if (!string.IsNullOrWhiteSpace(dto.Middel_name) && !dto.Middel_name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "Middle name must contain only letters." });

            if (!dto.Last_Name.All(char.IsLetter))
                return BadRequest(new { status = 400, message = "Last name must contain only letters." });

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(dto.Email) || !dto.Email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { status = 400, message = "Invalid email format." });
            }

            var exists = await _context.Doctorss.AnyAsync(d => d.Email == dto.Email);
            if (exists)
            {
                return Conflict(new { status = 409, message = "Email already registered." });
            }

            var verificationCode = GenerateVerificationCode();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var labDoctor = new Doctors
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

            _context.Doctorss.Add(labDoctor);
            await _context.SaveChangesAsync();

            try
            {
                var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    _context.Doctorss.Remove(labDoctor);
                    await _context.SaveChangesAsync();
                    return BadRequest(new { status = 400, message = "Failed to create identity.", errors = result.Errors });
                }

                labDoctor.IdentityUserId = user.Id;
                _context.Doctorss.Update(labDoctor);
                await _context.SaveChangesAsync();

                if (!await _roleManager.RoleExistsAsync("LabDoctor"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("LabDoctor"));
                }

                await _userManager.AddToRoleAsync(user, "LabDoctor");
                await SendVerificationEmail(dto.Email, verificationCode);
            }
            catch (Exception ex)
            {
                _context.Doctorss.Remove(labDoctor);
                await _context.SaveChangesAsync();

                return StatusCode(500, new
                {
                    status = 500,
                    message = "Failed to send verification email.",
                    error = ex.Message
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Lab doctor registered successfully. Please check your email to verify your account."
            });
        }
        [HttpPost("login/lab-doctor")]
        public async Task<IActionResult> LoginLabDoctor([FromForm] LabDoctorLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { status = 400, message = "Email and password are required." });
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new { status = 401, message = "Invalid credentials." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("LabDoctor"))
            {
                return Forbid();
            }

            var labDoctor = await _context.Doctorss.FirstOrDefaultAsync(d => d.Email == dto.Email);
            if (labDoctor == null || !(labDoctor.IsVerified ?? false))
            {
                return Unauthorized(new { status = 401, message = "Lab doctor not verified or not found." });
            }

           
            var (token, expiration, refreshToken) = GenerateJwtToken(labDoctor.Id.ToString(), user.Email, "LabDoctor");

           
            labDoctor.RefreshToken = refreshToken;
            labDoctor.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token,
                expiresAt = expiration,
                refreshToken,
                role = "LabDoctor",
                doctorId = labDoctor.Id,
                name = $"{labDoctor.First_Name} {labDoctor.Last_Name}"
            });
        }

        [HttpPost("registerRadiographyDoctor")]
        public async Task<IActionResult> RegisterRadiographyDoctor([FromForm] DoctorRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.First_Name) ||
                string.IsNullOrWhiteSpace(dto.Last_Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { status = 400, message = "All fields are required." });
            }

            if (!dto.First_Name.All(char.IsLetter) ||
                (!string.IsNullOrWhiteSpace(dto.Middel_name) && !dto.Middel_name.All(char.IsLetter)) ||
                !dto.Last_Name.All(char.IsLetter))
            {
                return BadRequest(new { status = 400, message = "Name fields must contain only letters." });
            }

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(dto.Email) || !dto.Email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { status = 400, message = "Invalid email format." });
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

                if (!await _roleManager.RoleExistsAsync("RadiographyDoctor"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("RadiographyDoctor"));
                }

                await _userManager.AddToRoleAsync(user, "RadiographyDoctor");
                await SendVerificationEmail(doctor.Email, verificationCode);
            }
            catch (Exception ex)
            {
                _context.Doctorss.Remove(doctor);
                await _context.SaveChangesAsync();

                return StatusCode(500, new
                {
                    status = 500,
                    message = "Failed to send verification email.",
                    error = ex.Message
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Radiography doctor registered successfully. Please check your email to verify your account."
            });
        }
        [HttpPost("login/radiography-doctor")]
        public async Task<IActionResult> LoginRadiographyDoctor([FromForm] RadiographyDoctLogindto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { status = 400, message = "Email and password are required." });
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new { status = 401, message = "Invalid credentials." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("RadiographyDoctor"))
            {
                return Forbid();
            }

            var doctor = await _context.Doctorss.FirstOrDefaultAsync(d => d.Email == dto.Email);
            if (doctor == null || !doctor.IsVerified.GetValueOrDefault())
            {
                return Unauthorized(new { status = 401, message = "Doctor not verified or not found." });
            }

            var (token, expiration, refreshToken) = GenerateJwtToken(doctor.Id.ToString(), user.Email, "RadiographyDoctor");

          
            doctor.RefreshToken = refreshToken;
            doctor.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                token,
                expiresAt = expiration,
                refreshToken,
                role = "RadiographyDoctor",
                doctorId = doctor.Id,
                name = $"{doctor.First_Name} {doctor.Last_Name}"
            });
        }


        [HttpPost("refresh-token-Lab_Radiography_Doctor")]
        public async Task<IActionResult> RefreshTokenForLabAndRadiographyDoctor([FromForm] TokenRequestDto tokenRequest)
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

            
            if (role != "LabDoctor" && role != "RadiographyDoctor")
            {
                return Unauthorized(new { status = 401, message = "Not authorized as LabDoctor or RadiographyDoctor." });
            }

           
            if (!long.TryParse(userId, out long doctorId))
            {
                return BadRequest(new { status = 400, message = "Invalid user ID in token." });
            }

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
            {
                return NotFound(new { status = 404, message = "Doctor not found." });
            }

            if (doctor.RefreshToken != tokenRequest.RefreshToken || doctor.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { status = 400, message = "Invalid or expired refresh token." });
            }

            var (newToken, expiration, newRefreshToken) = GenerateJwtToken(userId, email, role);

            doctor.RefreshToken = newRefreshToken;
            doctor.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
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
        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]

        [HttpPost("notify-patients-delay/{dateId}")]
        public async Task<IActionResult> NotifyPatientsOfDelay([FromRoute] long dateId, [FromForm] int delayMinutes)
        {
            if (delayMinutes <= 0)
            {
                return BadRequest(new { status = 400, message = "Delay minutes must be greater than zero." });
            }

            var date = await _context.Datess.FindAsync(dateId);
            if (date == null)
            {
                return NotFound(new { status = 404, message = "Date not found." });
            }

            var reservations = await _context.Consulting_reservations
                .Where(r => r.DateId == dateId)
                .Include(r => r.Patient)
                .ToListAsync();

            if (!reservations.Any())
            {
                return NotFound(new { status = 404, message = "No reservations found for this date." });
            }

            var doctor = await _context.Doctorss.FindAsync(date.DoctorId);
            var doctorName = doctor != null
                ? $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}".Trim()
                : "your doctor";

            foreach (var reservation in reservations)
            {
                var patient = reservation.Patient;
                var message = $"نأسف، سيتم تأخير موعدك مع الدكتور {doctorName} بمقدار {delayMinutes} دقيقة.";

                Console.WriteLine($"🔔 إشعار للمريض {patient.First_Name} {patient.Last_Name}: {message}");

               
            }


            return Ok(new
            {
                status = 200,
                message = "Delay notifications sent successfully.",
                totalPatients = reservations.Count
            });
        }

        [HttpGet("clinic-doctors/{clinicId}")]
        public async Task<IActionResult> GetDoctorsByClinic(int clinicId)
        {
            var doctors = await _context.Doctorss
                .Where(d => d.ClinicId == clinicId)
                .Select(d => new
                {
                    FullName = d.First_Name + " " + d.Middel_name + " " + d.Last_Name,
                    PhoneNumber = d.PhoneNumber,
                    ClinicName = _context.Clinicscss
                                         .Where(c => c.Id == d.ClinicId)
                                         .Select(c => c.Clinic_Name)
                                         .FirstOrDefault(),
                    Overview = d.Overview
                })
                .ToListAsync();

            if (doctors == null || !doctors.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "No doctors available for the specified clinic."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Doctors for the specified clinic retrieved successfully.",
                doctors = doctors
            });
        }
        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]
        [HttpGet("has-clinic")]
public async Task<IActionResult> HasClinic()
{
    var userIdString = User.FindFirst("userId")?.Value;

    if (!long.TryParse(userIdString, out long doctorId))
    {
        return Unauthorized(new { message = "Invalid userId in token." });
    }

    var doctor = await _context.Doctorss
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == doctorId);

    if (doctor == null)
    {
        return NotFound(new { message = "Doctor not found for the current user.", userIdInToken = doctorId });
    }

    bool hasClinic = doctor.ClinicId != null;

    return Ok(new { hasClinic });
}
        [Authorize(Roles ="Manager")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDoctorsrecords()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var doctors = await _context.Doctorss
                .Include(d => d.Clinic)
                .Select(d => new
                {
                    d.Id,
                    FullName = $"{d.First_Name} {d.Middel_name} {d.Last_Name}",
                    d.PhoneNumber,
                    d.Residence,
                    d.Overview,
                    ClinicName = d.Clinic != null ? d.Clinic.Clinic_Name : "N/A",
                    pdf = d.PdfFile != null ? $"{baseUrl}/api/doctors/pdf/{d.Id}" : null,
                    photo = !string.IsNullOrEmpty(d.ImagePath) ? $"{baseUrl}/{d.ImagePath}" : null
                })
                .ToListAsync();

            if (!doctors.Any())
            {
                return Ok(new
                {
                    status = 200,
                    message = "No doctors found.",
                    data = new List<object>()
                });
            }

            return Ok(new
            {
                status = 200,
                message = "All doctors retrieved successfully.",
                data = doctors
            });
        }




    }
}
