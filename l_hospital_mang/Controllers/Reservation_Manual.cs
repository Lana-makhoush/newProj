using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using l_hospital_mang.DTOs;
using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Reservation_Manual : ControllerBase
    {
        private readonly AppDbContext _context;

        public Reservation_Manual(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Secretary")]
        [HttpPost("AddReservationManualy/{patientId}")]
        public async Task<IActionResult> AddReservationManualy(long patientId, [FromForm] ConsultingReservationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            var reservation = new Consulting_reservation
            {
                ReservationType = dto.ReservationType,
                Price = dto.Price.Value, 
                PatientId = patientId
            };

            try
            {
                _context.Consulting_reservations.Add(reservation);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Reservation added successfully",
                    data = new
                    {
                        reservation.Id,
                        reservation.ReservationType,
                        reservation.Price,
                        reservation.PatientId,
                        patient = new
                        {
                            patient.First_Name,
                            patient.Middel_name,
                            patient.Last_Name,
                            Age = patient.Age?.ToString("yyyy-MM-dd"),
                            patient.Residence,
                            patient.ID_Number,
                            patient.PhoneNumber
                        }
                    }
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

        [HttpPut("UpdateReservationManualy/{reservationId}")]
        public async Task<IActionResult> UpdateReservationManualy(long reservationId, [FromForm] UpdateConsultingReservationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reservation = await _context.Consulting_reservations.FindAsync(reservationId);
            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            if (!string.IsNullOrWhiteSpace(dto.ReservationType))
                reservation.ReservationType = dto.ReservationType;

            if (dto.Price.HasValue)
                reservation.Price = dto.Price.Value;

            try
            {
                await _context.SaveChangesAsync();

                var patient = await _context.Patients.FindAsync(reservation.PatientId);

                return Ok(new
                {
                    message = "Reservation updated successfully",
                    data = new
                    {
                        reservation.Id,
                        reservation.ReservationType,
                        reservation.Price,
                        reservation.PatientId,
                        patient = new
                        {
                            patient.First_Name,
                            patient.Middel_name,
                            patient.Last_Name,
                            Age = patient.Age?.ToString("yyyy-MM-dd"),
                            patient.Residence,
                            patient.ID_Number,
                            patient.PhoneNumber
                        }
                    }
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

        [HttpDelete("DeleteReservationManualy/{reservationId}")]
        public async Task<IActionResult> DeleteReservationManualy(long reservationId)
        {
            var reservation = await _context.Consulting_reservations.FindAsync(reservationId);
            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            try
            {
                _context.Consulting_reservations.Remove(reservation);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Reservation deleted successfully",
                   
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
        [HttpGet("GetAllReservations")]
        public IActionResult GetAllReservations()
        {
            var reservations = _context.Consulting_reservations
                .Include(r => r.Patient)
                .ToList()
                .Select(reservation => new
                {
                    id = reservation.Id,
                    reservationType = reservation.ReservationType,
                    price = reservation.Price,
                    patientId = reservation.PatientId,
                    patient = reservation.Patient == null ? null : new
                    {
                        first_Name = reservation.Patient.First_Name,
                        middel_name = reservation.Patient.Middel_name,
                        last_Name = reservation.Patient.Last_Name,
                        age = reservation.Patient.Age?.ToString("yyyy-MM-dd"),
                        residence = reservation.Patient.Residence,
                        iD_Number = reservation.Patient.ID_Number,
                        phoneNumber = reservation.Patient.PhoneNumber
                    }
                })
                .ToList();

            if (!reservations.Any())
            {
                return NotFound(new
                {
                    message = "No reservations found."
                });
            }

            return Ok(new
            {
                message = "Reservations retrieved successfully.",
                data = reservations
            });
        }


        [HttpGet("GetReservationById/{reservationId}")]
        public async Task<IActionResult> GetReservationById(long reservationId)
        {
            var reservation = await _context.Consulting_reservations
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            return Ok(new
            {
                id = reservation.Id,
                reservationType = reservation.ReservationType,
                price = reservation.Price,
                patientId = reservation.PatientId,
                patient = new
                {
                    first_Name = reservation.Patient.First_Name,
                    middel_name = reservation.Patient.Middel_name,
                    last_Name = reservation.Patient.Last_Name,
                    age = reservation.Patient.Age?.ToString("yyyy-MM-dd"),
                    residence = reservation.Patient.Residence,
                    iD_Number = reservation.Patient.ID_Number,
                    phoneNumber = reservation.Patient.PhoneNumber
                }
            });
        }




    }
}
