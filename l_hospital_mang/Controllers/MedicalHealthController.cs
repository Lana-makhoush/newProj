using Microsoft.AspNetCore.Mvc;
using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using Microsoft.EntityFrameworkCore;
using l_hospital_mang.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace l_hospital_mang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalHealthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicalHealthController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Doctor,Manager")]

        [HttpPost("{patientId}")]
        public async Task<IActionResult> AddMedicalRecord(long patientId, [FromForm] MedicalHealthDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            var medicalRecord = new Medical_Health
            {
                Main_Complain = model.Main_Complain,
                Pathological_story = model.Pathological_story,
                Clinical_examination = model.Clinical_examination,
                Surveillance = model.Surveillance,
                Diagnosis = model.Diagnosis,
                Treatment = model.Treatment,
                plan = model.plan,
                notes = model.notes,
                PatientId = patientId
            };

            _context.Medical_Healths.Add(medicalRecord);
            await _context.SaveChangesAsync();

            var result = await _context.Medical_Healths
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == medicalRecord.Id);

            return Ok(new
            {
                message = "Medical record added successfully.",
                data = new
                {
                    result.Id,
                    result.Main_Complain,
                    result.Diagnosis,
                    result.Treatment,
                    result.notes,
                    Patient = result.Patient != null ? new
                    {
                        result.Patient.Id,
                        result.Patient.First_Name,
                        result.Patient.Middel_name,
                        result.Patient.Last_Name,
                        result.Patient.Age,
                        result.Patient.Residence,
                        result.Patient.ID_Number,
                        result.Patient.PhoneNumber,
                        result.Patient.ImagePath
                    } : null
                }
            });
        }
        [Authorize(Roles = "Doctor,Manager")]

        [HttpPut("{patientId}/medical-record/{id}")]
        public async Task<IActionResult> UpdateMedicalRecord(long patientId, long id, [FromForm] UpdateMedicalHealthDTO dto)
        {
            var medicalRecord = await _context.Medical_Healths
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.PatientId == patientId);

            if (medicalRecord == null)
                return NotFound(new { message = "Medical record not found for this patient." });

            if (!string.IsNullOrWhiteSpace(dto.Main_Complain))
                medicalRecord.Main_Complain = dto.Main_Complain;

            if (!string.IsNullOrWhiteSpace(dto.Pathological_story))
                medicalRecord.Pathological_story = dto.Pathological_story;

            if (!string.IsNullOrWhiteSpace(dto.Clinical_examination))
                medicalRecord.Clinical_examination = dto.Clinical_examination;

            if (!string.IsNullOrWhiteSpace(dto.Surveillance))
                medicalRecord.Surveillance = dto.Surveillance;

            if (!string.IsNullOrWhiteSpace(dto.Diagnosis))
                medicalRecord.Diagnosis = dto.Diagnosis;

            if (!string.IsNullOrWhiteSpace(dto.Treatment))
                medicalRecord.Treatment = dto.Treatment;

            if (!string.IsNullOrWhiteSpace(dto.plan))
                medicalRecord.plan = dto.plan;

            if (!string.IsNullOrWhiteSpace(dto.notes))
                medicalRecord.notes = dto.notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Medical record updated successfully.",
                data = new
                {
                    medicalRecord.Id,
                    medicalRecord.Main_Complain,
                    medicalRecord.Pathological_story,
                    medicalRecord.Clinical_examination,
                    medicalRecord.Surveillance,
                    medicalRecord.Diagnosis,
                    medicalRecord.Treatment,
                    medicalRecord.plan,
                    medicalRecord.notes,
                    Patient = new
                    {
                        medicalRecord.Patient.Id,
                        FullName = $"{medicalRecord.Patient.First_Name} {medicalRecord.Patient.Middel_name} {medicalRecord.Patient.Last_Name}",
                        medicalRecord.Patient.PhoneNumber,
                        medicalRecord.Patient.Residence
                    }
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalHealthDTO>> GetMedicalHealth(long id)
        {
            var health = await _context.Medical_Healths
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (health == null)
                return NotFound();

            var dto = new MedicalHealthDTO
            {
                Id = health.Id,
                Main_Complain = health.Main_Complain,
                Pathological_story = health.Pathological_story,
                Clinical_examination = health.Clinical_examination,
                Surveillance = health.Surveillance,
                Diagnosis = health.Diagnosis,
                Treatment = health.Treatment,
                plan = health.plan,
                notes = health.notes,
                PatientId = health.PatientId,
                Patient = health.Patient != null ? new PatientDTO
                {
                    Id = health.Patient.Id,
                    First_Name = health.Patient.First_Name,
                    Middel_name = health.Patient.Middel_name,
                    Last_Name = health.Patient.Last_Name,
                    Age = health.Patient.Age,
                    Residence = health.Patient.Residence,
                    ID_Number = health.Patient.ID_Number,
                    PhoneNumber = health.Patient.PhoneNumber,
                    ImagePath = health.Patient.ImagePath
                } : null
            };

            return Ok(dto);
        }


        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<MedicalHealthDTO>>> GetAllMedicalHealths()
        {
            var healths = await _context.Medical_Healths
                .Include(m => m.Patient)
                .ToListAsync();

            var dtoList = healths.Select(health => new MedicalHealthDTO
            {
                Id = health.Id,
                Main_Complain = health.Main_Complain,
                Pathological_story = health.Pathological_story,
                Clinical_examination = health.Clinical_examination,
                Surveillance = health.Surveillance,
                Diagnosis = health.Diagnosis,
                Treatment = health.Treatment,
                plan = health.plan,
                notes = health.notes,
                PatientId = health.PatientId,
                Patient = health.Patient != null ? new PatientDTO
                {
                    Id = health.Patient.Id,
                    First_Name = health.Patient.First_Name,
                    Middel_name = health.Patient.Middel_name,
                    Last_Name = health.Patient.Last_Name,
                    Age = health.Patient.Age,
                    Residence = health.Patient.Residence,
                    ID_Number = health.Patient.ID_Number,
                    PhoneNumber = health.Patient.PhoneNumber,
                    ImagePath = health.Patient.ImagePath
                } : null
            });

            return Ok(dtoList);
        }
        [Authorize(Roles = "Doctor,Manager")]


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMedicalHealth(long id)
        {
            var health = await _context.Medical_Healths.FindAsync(id);

            if (health == null)
                return NotFound(new { message = $"Medical health record  not found." });

            _context.Medical_Healths.Remove(health);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Medical health record with  deleted successfully." });
        }


    }
}
