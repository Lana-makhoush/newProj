using l_hospital_mang.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using l_hospital_mang.Data;
using System.Linq;
using l_hospital_mang.DTOs;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Resident_patientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public Resident_patientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-patient-simple")]
        public async Task<IActionResult> AddResidentPatientSimple([FromForm] Resident_patients patient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Validation failed.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            bool idExists = await _context.Resident_patientss
                .AnyAsync(p => p.ID_Number == patient.ID_Number);
            if (idExists)
            {
                return Conflict(new
                {
                    StatusCode = 409,
                    Message = "ID Number already exists."
                });
            }

            try
            {
                _context.Resident_patientss.Add(patient);
                await _context.SaveChangesAsync();

                return Created("", new
                {
                    StatusCode = 201,
                    Message = "Resident patient added successfully.",
                    Data = new
                    {
                        patient.Id,
                        patient.First_Name,
                        patient.Middel_name,
                        patient.Last_Name,
                        Age = patient.Age?.ToString("yyyy-MM-dd"), 
                        patient.Residence,
                        patient.ID_Number,
                        patient.PhoneNumber
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while saving the patient.",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResidentPatient(int id, [FromForm] ResidentPatientUpdateDto dto)
        {
            var patient = await _context.Resident_patientss.FindAsync(id);

            if (patient == null)
                return NotFound(new { message = "Resident patient not found." });

            if (!string.IsNullOrWhiteSpace(dto.First_Name))
                patient.First_Name = dto.First_Name;

            if (!string.IsNullOrWhiteSpace(dto.Middel_name))
                patient.Middel_name = dto.Middel_name;

            if (!string.IsNullOrWhiteSpace(dto.Last_Name))
                patient.Last_Name = dto.Last_Name;

            if (dto.Age.HasValue)
            {
                if (dto.Age.Value > DateTime.Now)
                {
                    return BadRequest(new { message = "Age cannot be a future date." });
                }
                patient.Age = dto.Age.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Residence))
                patient.Residence = dto.Residence;

            if (!string.IsNullOrWhiteSpace(dto.ID_Number))
                patient.ID_Number = dto.ID_Number;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                patient.PhoneNumber = dto.PhoneNumber;

            if (dto.RoomId.HasValue)
                patient.RoomId = dto.RoomId;

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Resident patient updated successfully.",
                    Data = new
                    {
                        patient.Id,
                        patient.First_Name,
                        patient.Middel_name,
                        patient.Last_Name,
                        Age = patient.Age?.ToString("yyyy-MM-dd"),
                        patient.Residence,
                        patient.ID_Number,
                        patient.PhoneNumber,
                        patient.RoomId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error updating resident patient.",
                    Error = ex.Message
                });
            }
        }


        [HttpDelete("delete-patient/{id}")]
        public async Task<IActionResult> DeleteResidentPatient(int id)
        {
            var patient = await _context.Resident_patientss.FindAsync(id);

            if (patient == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = $"Patient with ID {id} not found."
                });
            }

            try
            {
                _context.Resident_patientss.Remove(patient);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Resident patient deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while deleting the patient.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("assign-room/{patientId}/{roomId}")]
        public async Task<IActionResult> AssignRoom(int patientId, int roomId)
        {
            var patient = await _context.Resident_patientss.FindAsync(patientId);
            if (patient == null)
                return NotFound();

            patient.RoomId = roomId;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "Room assigned to resident patient successfully.",
                data = new
                {
                    patient.Id,
                    patient.First_Name,
                    patient.Middel_name,
                    patient.Last_Name,
                    patient.Age,
                    patient.Residence,
                    patient.ID_Number,
                    patient.PhoneNumber,
                    patient.RoomId
                }
            });
        }



        [HttpPost("submit-selection/{RoomId}/{ResidentPatientId}")]
        public async Task<IActionResult> SubmitSelection([FromRoute] int RoomId, [FromRoute] int ResidentPatientId)
        {
            var patient = await _context.Resident_patientss.FindAsync(ResidentPatientId);
            if (patient == null)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    message = "Resident patient not found."
                });
            }

            var room = await _context.Room.FindAsync(RoomId);
            if (room == null)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    message = "Room not found."
                });
            }

            patient.RoomId = RoomId;

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    message = "Room assigned to resident patient successfully.",
                    data = new
                    {
                        patient.Id,
                        patient.First_Name,
                        patient.Middel_name,
                        patient.Last_Name,
                        patient.Age,
                        patient.Residence,
                        patient.ID_Number,
                        patient.PhoneNumber,
                        patient.RoomId
                    }
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while assigning the room.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("all-patients")]
        public async Task<IActionResult> GetAllResidentPatients()
        {
            var patients = await _context.Resident_patientss
                .Select(p => new
                {
                    p.Id,
                    p.First_Name,
                    p.Middel_name,
                    p.Last_Name,
                    p.Age,
                    p.Residence,
                    p.ID_Number,
                    p.PhoneNumber,
                    p.RoomId
                })
                .ToListAsync();

            if (patients == null || !patients.Any())
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No resident patients available."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Resident patients retrieved successfully.",
                Data = patients
            });
        }
        [HttpGet("patient/{id}")]
        public async Task<IActionResult> GetResidentPatientById(int id)
        {
            var patient = await _context.Resident_patientss
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.First_Name,
                    p.Middel_name,
                    p.Last_Name,
                    p.Age,
                    p.Residence,
                    p.ID_Number,
                    p.PhoneNumber,
                    p.RoomId
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Resident patient not found."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Resident patient retrieved successfully.",
                Data = patient
            });
        }



    }
}
