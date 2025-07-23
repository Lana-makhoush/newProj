using l_hospital_mang.Data;
using l_hospital_mang.Data.DTOs;
using l_hospital_mang.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using l_hospital_mang.DTOs;
//using l_hospital_mang.Migrations;
using Analysis = l_hospital_mang.Data.Models.Analysis;
using Consulting_reservation = l_hospital_mang.Data.Models.Consulting_reservation;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text.RegularExpressions;
namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AnalysisController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Authorize(Roles = "LabDoctor")]

[HttpPost("add/{consultingReservationId}")]
    public async Task<IActionResult> AddAnalysis([FromRoute] int consultingReservationId, [FromForm] AnalysisDTTo dto)
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

       
        var nameRegex = new Regex("^[a-zA-Z]+$");

        if (!nameRegex.IsMatch(dto.First_Name))
        {
            return BadRequest(new { StatusCode = 400, Message = "First name must contain only letters." });
        }

        if (!nameRegex.IsMatch(dto.Middel_name))
        {
            return BadRequest(new { StatusCode = 400, Message = "Middle name must contain only letters." });
        }

        if (!nameRegex.IsMatch(dto.Last_Name))
        {
            return BadRequest(new { StatusCode = 400, Message = "Last name must contain only letters." });
        }

      
        if (dto.Price < 100)
        {
            return BadRequest(new { StatusCode = 400, Message = "Price must be at least 100." });
        }

       
        if (dto.PdfFile == null || dto.PdfFile.Length == 0 || dto.PdfFile.ContentType != "application/pdf")
        {
            return BadRequest(new
            {
                StatusCode = 400,
                Message = "A valid PDF file is required."
            });
        }

       
        var consultingReservation = await _context.Set<Consulting_reservation>()
            .Include(c => c.Patient)
            .FirstOrDefaultAsync(c => c.Id == consultingReservationId);

        if (consultingReservation == null)
        {
            return NotFound(new
            {
                StatusCode = 404,
                Message = "Consulting reservation not found."
            });
        }

        var existingAnalysis = await _context.Set<Analysis>()
            .FirstOrDefaultAsync(a => a.Consulting_reservationId == consultingReservationId);

        if (existingAnalysis != null)
        {
            return Conflict(new
            {
                StatusCode = 409,
                Message = $"Analysis record for Consulting_reservationId {consultingReservationId} already exists."
            });
        }

        if (string.IsNullOrEmpty(_env.WebRootPath))
        {
            return StatusCode(500, new { Message = "WebRootPath is not configured." });
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.PdfFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await dto.PdfFile.CopyToAsync(fileStream);
        }

        byte[] pdfBytes;
        using (var ms = new MemoryStream())
        {
            await dto.PdfFile.CopyToAsync(ms);
            pdfBytes = ms.ToArray();
        }

        var relativeFilePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");

        var analysis = new Analysis
        {
            First_Name = dto.First_Name ?? consultingReservation.Patient.First_Name,
            Middel_name = dto.Middel_name ?? consultingReservation.Patient.Middel_name,
            Last_Name = dto.Last_Name ?? consultingReservation.Patient.Last_Name,
            Age = dto.Age,
            PdfFile = pdfBytes,
            PdfFilePath = relativeFilePath,
            Consulting_reservationId = consultingReservationId,
            Price = dto.Price
        };

        try
        {
            _context.Set<Analysis>().Add(analysis);
            await _context.SaveChangesAsync();

            string base64Pdf = Convert.ToBase64String(pdfBytes);

            return Ok(new
            {
                StatusCode = 200,
                Message = "Analysis record added successfully.",
                Data = new
                {
                    analysis.Id,
                    analysis.First_Name,
                    analysis.Middel_name,
                    analysis.Last_Name,
                    Age = analysis.Age.HasValue ? analysis.Age.Value.ToString("yyyy-MM-dd") : null,
                    analysis.Consulting_reservationId,
                    analysis.Price,
                    PdfFileBase64 = base64Pdf,
                    PdfUrl = $"{Request.Scheme}://{Request.Host}/{relativeFilePath}"
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                StatusCode = 500,
                Message = "An error occurred while saving the analysis record.",
                Error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }


    [Authorize(Roles = "LabDoctor")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAnalysis(long id, [FromForm] AnalysisUpdateDtocs dto)
        {
            var analysis = await _context.Analysiss.FindAsync(id);
            if (analysis == null)
            {
                return NotFound(new { message = "Analysis not found." });
            }

            bool IsValidName(string name)
            {
                
                return Regex.IsMatch(name, @"^[a-zA-Z]+$");
            }

            if (!string.IsNullOrEmpty(dto.First_Name))
            {
                if (!IsValidName(dto.First_Name))
                    return BadRequest(new { message = "First_Name must contain only letters." });

                analysis.First_Name = dto.First_Name;
            }

            if (!string.IsNullOrEmpty(dto.Middel_name))
            {
                if (!IsValidName(dto.Middel_name))
                    return BadRequest(new { message = "Middel_name must contain only letters." });

                analysis.Middel_name = dto.Middel_name;
            }

            if (!string.IsNullOrEmpty(dto.Last_Name))
            {
                if (!IsValidName(dto.Last_Name))
                    return BadRequest(new { message = "Last_Name must contain only letters." });

                analysis.Last_Name = dto.Last_Name;
            }

            if (dto.Age.HasValue)
                analysis.Age = dto.Age.Value;

            if (dto.Price > 0)
                analysis.Price = dto.Price;

            if (dto.PdfFile != null && dto.PdfFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.PdfFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.PdfFile.CopyToAsync(fileStream);
                }

                analysis.PdfFilePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");

                using (var ms = new MemoryStream())
                {
                    await dto.PdfFile.CopyToAsync(ms);
                    analysis.PdfFile = ms.ToArray();
                }
            }

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Analysis updated successfully.",
                    data = new
                    {
                        analysis.Id,
                        analysis.First_Name,
                        analysis.Middel_name,
                        analysis.Last_Name,
                        analysis.Age,
                        analysis.Price,
                        PdfFileBase64 = analysis.PdfFile != null ? Convert.ToBase64String(analysis.PdfFile) : null,
                        PdfFileUrl = string.IsNullOrEmpty(analysis.PdfFilePath) ? null : $"{Request.Scheme}://{Request.Host}/{analysis.PdfFilePath}"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the analysis.", error = ex.Message });
            }
        }



        [Authorize(Roles = "LabDoctor")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAnalysis(long id)
        {
            var existingAnalysis = await _context.Analysiss.FindAsync(id);
            if (existingAnalysis == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Analysis not found."
                });
            }

            try
            {
                _context.Analysiss.Remove(existingAnalysis);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Analysis deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while deleting the analysis.",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAnalysisById(long id)
        {
            var analysis = await _context.Analysiss.FindAsync(id);
            if (analysis == null)
            {
                return NotFound(new { message = "Analysis not found." });
            }

            string base64Pdf = analysis.PdfFile != null ? Convert.ToBase64String(analysis.PdfFile) : null;

            return Ok(new
            {
                analysis.Id,
                analysis.First_Name,
                analysis.Middel_name,
                analysis.Last_Name,
                Age = analysis.Age?.ToString("yyyy-MM-dd"),
                analysis.Consulting_reservationId,
                analysis.Price,
                PdfFileBase64 = base64Pdf
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAnalyses()
        {
            var analyses = await _context.Analysiss.ToListAsync();

            var result = analyses.Select(analysis => new
            {
                analysis.Id,
                analysis.First_Name,
                analysis.Middel_name,
                analysis.Last_Name,
                Age = analysis.Age?.ToString("yyyy-MM-dd"),
                analysis.Consulting_reservationId,
                analysis.Price,
                PdfFileBase64 = analysis.PdfFile != null ? Convert.ToBase64String(analysis.PdfFile) : null
            });

            return Ok(result);
        }

        [HttpGet("patients/{patientId}/latest-analyses/pdfs")]
        public async Task<IActionResult> GetLatestTwoAnalysisPdfs(long patientId)
        {
            var pdfFiles = await _context.Analysiss
                .Include(a => a.Consulting_reservation)
                .Where(a => a.Consulting_reservation.PatientId == patientId)
                .OrderByDescending(a => a.Id)
                .Take(2)
                .Select(a => a.PdfFile)
                .ToListAsync();

            if (!pdfFiles.Any())
            {
                return NotFound(new { message = "No analyses found to display." });
            }

            var base64Pdfs = pdfFiles
                .Where(p => p != null)
                .Select(p => Convert.ToBase64String(p))
                .ToList();

            return Ok(new
            {
                message = "Latest two analyses retrieved successfully.",
                pdfs = base64Pdfs
            });
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("my-analyses")]
        public async Task<IActionResult> GetAnalysesForPatient()
        {
            var patientIdClaim = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(patientIdClaim) || !long.TryParse(patientIdClaim, out long patientId))
            {
                return Unauthorized(new
                {
                    StatusCode = 401,
                    Message = "Unauthorized: Patient ID not found in token."
                });
            }

            var analyses = await _context.Analysiss
                .Include(a => a.Consulting_reservation) 
                .Where(a => a.Consulting_reservation.PatientId == patientId)
                .ToListAsync();

            if (analyses == null || !analyses.Any())
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No analysis records found for this patient."
                });
            }

            var result = analyses.Select(analysis => new
            {
                analysis.Id,
                analysis.First_Name,
                analysis.Middel_name,
                analysis.Last_Name,
                Age = analysis.Age?.ToString("yyyy-MM-dd"),
                analysis.Consulting_reservationId,
                analysis.Price,
                analysis.PdfFilePath,
                PdfFileBase64 = analysis.PdfFile != null ? Convert.ToBase64String(analysis.PdfFile) : null
            });

            return Ok(new
            {
                StatusCode = 200,
                Message = "Analysis records for the patient retrieved successfully.",
                Data = result
            });
        }
        [HttpGet("lab-doctors")]
        public async Task<IActionResult> GetLabDoctors()
        {
            var labRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "LabDoctor");

            if (labRole == null)
            {
                return NotFound(new { Message = "LabDoctor role not found." });
            }

            var labDoctorUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == labRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var labDoctors = await _context.Doctorss
                .Where(d => labDoctorUserIds.Contains(d.IdentityUserId))
                .Select(d => new
                {
                    d.Id,
                    d.First_Name,
                    d.Middel_name,
                    d.Last_Name,
                    d.Email,
                    d.PhoneNumber,
                    d.Residence,
                    d.Overview
                })
                .ToListAsync();

            if (labDoctors.Count == 0)
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "No Lab Doctors found."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Count = labDoctors.Count,
                Doctors = labDoctors
            });
        }

    }
}
