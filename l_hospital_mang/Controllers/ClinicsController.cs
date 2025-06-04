using l_hospital_mang.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using l_hospital_mang.Data;
using System.Text.RegularExpressions;
namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClinicsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/clinics/add-clinic
        [HttpPost("add-clinic")]
        public async Task<IActionResult> AddClinic([FromForm] Clinicscs clinic)
        {
            if (clinic == null || string.IsNullOrWhiteSpace(clinic.Clinic_Name))
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Clinic name is required."
                });

            var regex = new Regex(@"^[\p{L} ]+$");
            if (!regex.IsMatch(clinic.Clinic_Name))
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Clinic name must contain letters only."
                });

            bool exists = await _context.Clinicscss.AnyAsync(c => c.Clinic_Name == clinic.Clinic_Name);
            if (exists)
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Clinic name already exists."
                });

            _context.Clinicscss.Add(clinic);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "Clinic added successfully",
                data = new
                {
                    clinic.Id,
                    clinic.Clinic_Name
                }
            });
        }

        [HttpGet("all-clinics")]
        public async Task<IActionResult> GetAllClinics()
        {
            var clinics = await _context.Clinicscss
                .Select(c => new
                {
                    c.Id,
                    c.Clinic_Name
                })
                .ToListAsync();

            if (clinics == null || !clinics.Any())
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No clinics found."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Clinics retrieved successfully.",
                Data = clinics
            });
        }
        [HttpGet("clinic/{id}")]
        public async Task<IActionResult> GetClinicById(int id)
        {
            var clinic = await _context.Clinicscss
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Clinic_Name
                })
                .FirstOrDefaultAsync();

            if (clinic == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Clinic not found."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Clinic retrieved successfully.",
                Data = clinic
            });
        }

    }
}
