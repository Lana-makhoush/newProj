using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using l_hospital_mang.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurgeryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SurgeryController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Doctor,Manager")]

        [HttpPost("surgery-reservations/add/{patientId}")]
        public async Task<IActionResult> AddSurgeryReservation(long patientId, [FromForm] SurgeryReservationDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { statusCode = 400, message = "Validation failed.", errors = ModelState });

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            if (!model.SurgeryDate.HasValue)
                return BadRequest(new { message = "SurgeryDate is required." });

            var surgeryDate = model.SurgeryDate.Value.Date;

            if (surgeryDate < DateTime.Today)
                return BadRequest(new { message = "Surgery date must be today or in the future." });

            if (string.IsNullOrWhiteSpace(model.SurgeryTime))
                return BadRequest(new { message = "SurgeryTime is required." });

            if (!DateTime.TryParseExact(model.SurgeryTime.Trim(), "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime))
                return BadRequest(new { message = "Invalid time format. Use hh:mm AM/PM." });

            var requestedDateTime = surgeryDate.Add(parsedTime.TimeOfDay);

            if (surgeryDate == DateTime.Today && requestedDateTime <= DateTime.Now)
                return BadRequest(new { message = "Surgery time must be later than the current time." });

            if (string.IsNullOrWhiteSpace(model.SurgeryType))
                return BadRequest(new { message = "SurgeryType is required." });

            if (model.Price <= 0)
                return BadRequest(new { message = "Price must be positive." });

            if (model.DoctorId.HasValue)
            {
                var doctor = await _context.Doctorss.FindAsync(model.DoctorId.Value);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found." });

                var reservationsForDoctor = await _context.surgery_reservationss
                    .Where(r => r.DoctorId == model.DoctorId.Value && r.SurgeryDate == surgeryDate)
                    .ToListAsync();

                foreach (var r in reservationsForDoctor)
                {
                    if (DateTime.TryParseExact(r.SurgeryTime?.Trim(), "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var existingTime))
                    {
                        var existingDateTime = r.SurgeryDate.Date.Add(existingTime.TimeOfDay);
                        var minutesDifference = Math.Abs((existingDateTime - requestedDateTime).TotalMinutes);

                        if (minutesDifference < 120)
                        {
                            return BadRequest(new
                            {
                                message = $"Doctor has a conflicting surgery at {r.SurgeryTime}. Must be at least 2 hours apart."
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = $"Failed to parse existing surgery time: {r.SurgeryTime}" });
                    }
                }
            }

            var reservation = new surgery_reservations
            {
                PatientId = patientId,
                DoctorId = model.DoctorId,
                SurgeryDate = surgeryDate,
                SurgeryTime = model.SurgeryTime,
                SurgeryType = model.SurgeryType,
                Price = model.Price
            };

            _context.surgery_reservationss.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Surgery reservation created successfully.",
                data = new
                {
                    reservation.Id,
                    reservation.PatientId,
                    SurgeryDate = reservation.SurgeryDate.ToString("yyyy-MM-dd"),
                    reservation.SurgeryTime,
                    reservation.SurgeryType,
                    reservation.Price
                }
            });
        }


        [Authorize(Roles = "Doctor,Manager")]

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
                    patient = new
                    {
                        reservation.Patient.Id,
                        reservation.Patient.First_Name,
                        reservation.Patient.Middel_name,
                        reservation.Patient.Last_Name,
                        reservation.Patient.Age,
                        reservation.Patient.Residence,
                        reservation.Patient.ID_Number,
                        reservation.Patient.PhoneNumber,

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
        [Authorize(Roles = "Doctor,Manager")]

        [HttpPut("surgery-reservations/edit/{reservationId}/assign-doctor/{doctorId}")]
        public async Task<IActionResult> EditSurgeryReservation(long reservationId, long doctorId, [FromForm] EditSurgeryReservationDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation failed.", errors = ModelState });

            var reservation = await _context.surgery_reservationss.FindAsync(reservationId);
            if (reservation == null)
                return NotFound(new { message = "Surgery reservation not found." });

            var doctor = await _context.Doctorss.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            reservation.DoctorId = doctorId;

            if (model.SurgeryDate.HasValue)
            {
                if (model.SurgeryDate.Value.Date < DateTime.Today)
                    return BadRequest(new { message = "Surgery date must be today or in the future." });

                reservation.SurgeryDate = model.SurgeryDate.Value;
            }

            if (!string.IsNullOrWhiteSpace(model.SurgeryTime))
            {
                if (!DateTime.TryParseExact(model.SurgeryTime.Trim(), "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime))
                    return BadRequest(new { message = "Invalid time format. Use hh:mm AM/PM." });

                if (model.SurgeryDate.HasValue && model.SurgeryDate.Value.Date == DateTime.Today && parsedTime.TimeOfDay <= DateTime.Now.TimeOfDay)
                    return BadRequest(new { message = "Surgery time must be later than the current time." });

                reservation.SurgeryTime = model.SurgeryTime;
            }

            if (!string.IsNullOrWhiteSpace(model.SurgeryType))
            {
                reservation.SurgeryType = model.SurgeryType;
            }

            if (model.Price.HasValue)
            {
                if (model.Price <= 0)
                    return BadRequest(new { message = "Price must be greater than 0." });

                reservation.Price = model.Price.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Surgery reservation updated successfully.",
                data = new
                {
                    reservation.Id,
                    reservation.PatientId,
                    reservation.DoctorId,
                    SurgeryDate = reservation.SurgeryDate.ToString("yyyy-MM-dd"),
                    reservation.SurgeryTime,
                    reservation.SurgeryType,
                    reservation.Price
                }
            });
        }
        [Authorize(Roles = "Doctor,Manager")]

        [HttpDelete("surgery-reservations/delete/{reservationId}")]
        public async Task<IActionResult> DeleteSurgeryReservation(long reservationId)
        {
            var reservation = await _context.surgery_reservationss.FindAsync(reservationId);

            if (reservation == null)
                return NotFound(new { message = "Surgery reservation not found." });

            _context.surgery_reservationss.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Surgery reservation deleted successfully." });
        }
        [HttpGet("analyses/latest/{patientId}")]
        public async Task<IActionResult> GetLatestTwoAnalyses(long patientId)
        {
            var analyses = await _context.Analysiss
                .Where(a => a.Consulting_reservation != null && a.Consulting_reservation.PatientId == patientId)
                .OrderByDescending(a => a.Id)
                .Take(2)
                .Select(a => new
                {
                    a.Id,
                    a.PdfFile,
                    PdfFilePath = a.PdfFilePath ?? "not correct pdf path"
                })
                .ToListAsync();

            if (analyses == null || !analyses.Any())
            {
                return NotFound(new { message = "No analyses found for this patient." });
            }

            return Ok(new
            {
                message = "Latest two analyses retrieved successfully.",
                data = analyses
            });
        }




    }
}
