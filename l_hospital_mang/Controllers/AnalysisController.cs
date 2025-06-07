using l_hospital_mang.Data;
using l_hospital_mang.Data.DTOs;
using l_hospital_mang.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using l_hospital_mang.DTOs;
using l_hospital_mang.Migrations;
using Analysis = l_hospital_mang.Data.Models.Analysis;

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

        [HttpPost("add/{consultingReservationId}")]
        public async Task<IActionResult> AddAnalysis([FromRoute] int consultingReservationId, [FromForm] AnalysisDto dto)
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

            // حفظ الملف على السيرفر
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
                First_Name = consultingReservation.Patient.First_Name,
                Middel_name = consultingReservation.Patient.Middel_name,
                Last_Name = consultingReservation.Patient.Last_Name,
                Age = consultingReservation.Patient.Age,
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
                        Age = analysis.Age?.ToString("yyyy-MM-dd"),
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


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAnalysis(long id, [FromForm] AnalysisDto dto)
        {
            var analysis = await _context.Analysiss.FindAsync(id);
            if (analysis == null)
            {
                return NotFound(new { message = "Analysis not found." });
            }

            if (!string.IsNullOrEmpty(dto.First_Name))
                analysis.First_Name = dto.First_Name;

            if (!string.IsNullOrEmpty(dto.Middel_name))
                analysis.Middel_name = dto.Middel_name;

            if (!string.IsNullOrEmpty(dto.Last_Name))
                analysis.Last_Name = dto.Last_Name;

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


    }
}
