namespace l_hospital_mang.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    using System.Linq;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using global::l_hospital_mang.Data.Models;
    using global::l_hospital_mang.DTOs;

    using global::l_hospital_mang.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/[controller]")]
    public class DatesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DatesController(
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _environment = environment;
        }
        [Authorize(Roles = "Doctor,Manager")]
        [HttpPost("add-date")]
        public async Task<IActionResult> AddDate([FromForm] CreateDateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            int currentYear = DateTime.Now.Year;

            if (dto.Year < currentYear)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = $"Year must be {currentYear} or greater."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.ReservationType))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "ReservationType is required."
                });
            }

            if (dto.Price <= 0)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Price must be greater than zero."
                });
            }

            var newDate = new Dates
            {
                Day = dto.Day,
                Month = dto.Month,
                Year = dto.Year.Value,
                TimeOfDay = dto.TimeOfDay,
                ReservationType = dto.ReservationType,
                Price = dto.Price
            };

            _context.Datess.Add(newDate);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Date added successfully.",
                data = new
                {
                    id = newDate.Id,
                    day = newDate.Day,
                    month = newDate.Month,
                    year = newDate.Year,
                    timeOfDay = newDate.TimeOfDay,
                    reservationType = newDate.ReservationType,
                    price = newDate.Price
                }
            });
        }


        [Authorize(Roles = "Doctor,Manager")]
        [HttpPost("assign-doctor-to-date/{doctorId}/{dateId}")]
    public async Task<IActionResult> AssignDoctorToDate([FromRoute] long doctorId, [FromRoute] long dateId)
    {
        var doctor = await _context.Doctorss
            .Include(d => d.Clinic) 
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        if (doctor == null)
        {
            return NotFound(new
            {
                status = 404,
                message = $"Doctor not found."
            });
        }

        var date = await _context.Datess.FindAsync(dateId);
        if (date == null)
        {
            return NotFound(new
            {
                status = 404,
                message = $"Date not found."
            });
        }

        date.DoctorId = doctorId;

        _context.Datess.Update(date);
        await _context.SaveChangesAsync();

        string doctorFullName = $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}";

        string clinicName = doctor.Clinic?.Clinic_Name ?? "No Clinic";

        return Ok(new
        {
            status = 200,
            message = "Doctor assigned to the date successfully.",
            data = new
            {
                dateId = date.Id,
                doctorId = date.DoctorId,
                doctorFullName = doctorFullName,
                clinicName = clinicName,
                day = date.Day,
                month = date.Month,
                year = date.Year,
                timeOfDay = date.TimeOfDay
            }
        });
    }
        [Authorize(Roles = "Doctor,Manager")]

        [HttpPut("update-date/{dateId}")]
        public async Task<IActionResult> UpdateDate([FromRoute] long dateId, [FromForm] UpdateDateDto dto)
        {
            var date = await _context.Datess.FindAsync(dateId);
            if (date == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = $"Date with ID {dateId} not found."
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.Day))
                date.Day = dto.Day;

            if (dto.Month.HasValue)
                date.Month = dto.Month.Value;

            if (dto.Year.HasValue)
            {
                int currentYear = DateTime.Now.Year;
                if (dto.Year.Value < currentYear)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        message = $"Year must be {currentYear} or greater."
                    });
                }
                date.Year = dto.Year.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.TimeOfDay))
                date.TimeOfDay = dto.TimeOfDay;

            _context.Datess.Update(date);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Date updated successfully.",
                data = new
                {
                    id = date.Id,
                    day = date.Day,
                    month = date.Month,
                    year = date.Year,
                    timeOfDay = date.TimeOfDay
                }
            });
        }
        [Authorize(Roles = "Doctor,Manager")]

        [HttpGet("get-date/{dateId}")]
        public async Task<IActionResult> GetDateById([FromRoute] long dateId)
        {
            var date = await _context.Datess.FindAsync(dateId);
            if (date == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = $"Date  not found."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Date retrieved successfully.",
                data = new
                {
                    id = date.Id,
                    day = date.Day,
                    month = date.Month,
                    year = date.Year,
                    timeOfDay = date.TimeOfDay,
                    doctorId = date.DoctorId
                }
            });
        }

        [Authorize(Roles = "Doctor,Manager")]

        [HttpGet("all-dates")]
        public async Task<IActionResult> GetAllDates()
        {
            var dates = await _context.Datess
                .Include(d => d.Doctor)
                .ThenInclude(doc => doc.Clinic)
                .Select(d => new
                {
                    id = d.Id,
                    day = d.Day,
                    month = d.Month,
                    year = d.Year,
                    timeOfDay = d.TimeOfDay,
                    doctorId = d.DoctorId,
                    doctorFullName = d.Doctor != null
                        ? $"{d.Doctor.First_Name} {d.Doctor.Middel_name} {d.Doctor.Last_Name}"
                        : null,
                    clinicName = d.Doctor != null && d.Doctor.Clinic != null
                        ? d.Doctor.Clinic.Clinic_Name
                        : null
                })
                .ToListAsync();

            if (dates == null || dates.Count == 0)
            {
                return NotFound(new
                {
                    status = 404,
                    message = "no dates to display."
                });
            }

            return Ok(new
            {
                status = 200,
                count = dates.Count,
                data = dates
            });
        }

        [Authorize(Roles = "Doctor,Manager")]

        [HttpDelete("delete-date/{dateId}")]
        public async Task<IActionResult> DeleteDate([FromRoute] long dateId)
        {
            var date = await _context.Datess.FindAsync(dateId);
            if (date == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = $"Date not found."
                });
            }

            _context.Datess.Remove(date);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = $"Date  deleted successfully."
            });
        }
        [HttpGet("months-for-doctor/{doctorId}")]
        public async Task<IActionResult> GetDistinctMonthsForDoctor(long doctorId)
        {
            var months = await _context.Datess
                .Where(d => d.DoctorId == doctorId)
                .Select(d => d.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            if (months == null || !months.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "no available months!."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "available months",
                months = months
            });
        }
        [HttpGet("available-days/{doctorId}/{month}")]
        public async Task<IActionResult> GetAvailableDaysForDoctorInMonth(long doctorId, int month)
        {
            if (month < 1 || month > 12)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid month. Month should be between 1 and 12."
                });
            }

            var availableDays = await _context.Datess
                .Where(d => d.DoctorId == doctorId && d.Month == month)
                .Select(d => d.Day)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            if (availableDays == null || !availableDays.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "No available days for this doctor in the selected month."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Available days retrieved successfully.",
                days = availableDays
            });
        }

        [HttpGet("available-times/{doctorId}/{month}/{dayName}")]
        public async Task<IActionResult> GetAvailableTimesForDoctor(long doctorId, int month, string dayName)
        {
            var validDays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Saturday" };

            if (!validDays.Contains(dayName))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid day name. Valid days are: Sunday, Monday, Tuesday, Wednesday, Thursday, Saturday."
                });
            }

            if (month < 1 || month > 12)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid month. Month must be between 1 and 12."
                });
            }

            var times = await _context.Datess
                .Where(d => d.DoctorId == doctorId && d.Month == month && d.Day == dayName)
                .Select(d => d.TimeOfDay)
                .OrderBy(t => t)
                .ToListAsync();

            if (times == null || !times.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "No available times for this doctor on the selected day and month."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Available times retrieved successfully.",
                times = times
            });
        }
        [Authorize(Roles = "Patient")]
        [HttpPost("reserve/{doctorId}/{month}/{dayName}/{time}")]
        public async Task<IActionResult> ReserveConsultation(
    long doctorId, int month, string dayName, string time)
        {
            var patientIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(patientIdClaim) || !long.TryParse(patientIdClaim, out long patientId))
            {
                return Unauthorized(new { status = 401, message = "Unauthorized or invalid token." });
            }

            var validDays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Saturday" };
            if (!validDays.Contains(dayName))
            {
                return BadRequest(new { status = 400, message = "Invalid day name." });
            }

            if (month < 1 || month > 12)
            {
                return BadRequest(new { status = 400, message = "Invalid month." });
            }

            var matchedDate = await _context.Datess
                .Include(d => d.Doctor)
                .Where(d => d.DoctorId == doctorId && d.Month == month && d.Day == dayName && d.TimeOfDay == time)
                .FirstOrDefaultAsync();

            if (matchedDate == null)
            {
                return NotFound(new { status = 404, message = "No matching appointment found." });
            }

            var reservation = new Consulting_reservation
            {
                DateId = matchedDate.Id,
                PatientId = patientId,
                Price = matchedDate.Price,
                ReservationType = matchedDate.ReservationType ?? "Consultation"
            };

            _context.Consulting_reservations.Add(reservation);
            await _context.SaveChangesAsync();

            string doctorFullName = $"{matchedDate.Doctor?.First_Name} {matchedDate.Doctor?.Middel_name} {matchedDate.Doctor?.Last_Name}".Replace("  ", " ").Trim();

            return Ok(new
            {
                status = 200,
                message = "Reservation created successfully.",
                reservation = new
                {
                    reservation.Id,
                    reservation.ReservationType,
                    reservation.Price,
                    DateId = matchedDate.Id,
                    Day = matchedDate.Day,
                    Month = matchedDate.Month,
                    Year = matchedDate.Year,
                    Time = matchedDate.TimeOfDay,
                    DoctorName = doctorFullName
                }
            });
        }



    }
}


