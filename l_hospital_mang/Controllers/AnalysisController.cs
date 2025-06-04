using l_hospital_mang.Data;
using l_hospital_mang.Data.DTOs;
using l_hospital_mang.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalysisController(AppDbContext context)
        {
            _context = context;
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

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                await dto.PdfFile.CopyToAsync(ms);
                pdfBytes = ms.ToArray();
            }

            var analysis = new Analysis
            {
                First_Name = dto.First_Name,
                Middel_name = dto.Middel_name,
                Last_Name = dto.Last_Name,
                Age = dto.Age,
                PdfFile = pdfBytes,
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
                        analysis.Age,
                        analysis.Consulting_reservationId,
                        analysis.Price,
                        PdfFileBase64 = base64Pdf
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


    }
}
