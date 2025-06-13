using Microsoft.AspNetCore.Mvc;
using l_hospital_mang.DTOs;  // مكان وجود a_patient_dto
using System.Linq;
using l_hospital_mang.Data.Models;
using l_hospital_mang.Data;
using Microsoft.EntityFrameworkCore;
using l_hospital_mang.DTOs.l_hospital_mang.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientsController(AppDbContext context)
        {
            _context = context;
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






    }
}
