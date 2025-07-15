using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using l_hospital_mang.Data.Models;
using l_hospital_mang.Data;
using Microsoft.AspNetCore.Authorization;

namespace l_hospital_mang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RadiographyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RadiographyController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Authorize(Roles = "RadiographyDoctor")]

        [HttpPost("add/{consultingReservationId}")]
        public async Task<IActionResult> AddRadiography([FromRoute] long consultingReservationId, [FromForm] RadiographyDto dto)
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

            if (dto.Image == null || dto.Image.Length == 0 || !dto.Image.ContentType.StartsWith("image/"))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "A valid image file is required."
                });
            }

            var existingRadiography = await _context.Radiographyies
                .AnyAsync(r => r.Consulting_reservationId == consultingReservationId);

            if (existingRadiography)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "A radiography image for this consulting reservation already exists."
                });
            }

            var consultingReservation = await _context.Consulting_reservations
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

            if (string.IsNullOrEmpty(_env.WebRootPath))
            {
                return StatusCode(500, new { Message = "WebRootPath is not configured." });
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(fileStream);
            }

            var relativeImagePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");

            var radiography = new Radiography
            {
                First_Name = consultingReservation.Patient.First_Name,
                Middel_name = consultingReservation.Patient.Middel_name,
                Last_Name = consultingReservation.Patient.Last_Name,
                Age = consultingReservation.Patient.Age,
                Consulting_reservationId = consultingReservationId,
                Price = dto.Price,
                ImagePath = relativeImagePath
            };

            try
            {
                _context.Radiographyies.Add(radiography);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Radiography record added successfully.",
                    Data = new
                    {
                        radiography.Id,
                        radiography.First_Name,
                        radiography.Middel_name,
                        radiography.Last_Name,
                        Age = radiography.Age?.ToString("yyyy-MM-dd"),
                        radiography.Consulting_reservationId,
                        radiography.Price,
                        ImageUrl = $"{Request.Scheme}://{Request.Host}/{relativeImagePath}"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while saving the radiography record.",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [Authorize(Roles = "RadiographyDoctor")]


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateRadiography([FromRoute] long id, [FromForm] RadiographyUpdateDto dto)
        {
            var radiography = await _context.Radiographyies
                .Include(r => r.Consulting_reservation)
                .ThenInclude(c => c.Patient)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (radiography == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Radiography record not found."
                });
            }

            if (dto.Price.HasValue)
            {
                radiography.Price = dto.Price.Value;
            }

            if (dto.Image != null && dto.Image.Length > 0 && dto.Image.ContentType.StartsWith("image/"))
            {
                if (string.IsNullOrEmpty(_env.WebRootPath))
                {
                    return StatusCode(500, new { Message = "WebRootPath is not configured." });
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(radiography.ImagePath))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, radiography.ImagePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(fileStream);
                }

                radiography.ImagePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");
            }

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Radiography record updated successfully.",
                    Data = new
                    {
                        radiography.Id,
                        radiography.First_Name,
                        radiography.Middel_name,
                        radiography.Last_Name,
                        Age = radiography.Age?.ToString("yyyy-MM-dd"),
                        radiography.Consulting_reservationId,
                        radiography.Price,
                        ImageUrl = $"{Request.Scheme}://{Request.Host}/{radiography.ImagePath}"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while updating the radiography record.",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        [Authorize(Roles = "RadiographyDoctor")]


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteRadiography([FromRoute] long id)
        {
            var radiography = await _context.Radiographyies.FindAsync(id);
            if (radiography == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Radiography record not found."
                });
            }

            if (!string.IsNullOrEmpty(radiography.ImagePath))
            {
                var fullImagePath = Path.Combine(_env.WebRootPath, radiography.ImagePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(fullImagePath))
                {
                    System.IO.File.Delete(fullImagePath);
                }
            }

            _context.Radiographyies.Remove(radiography);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Radiography record deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while deleting the radiography record.",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRadiographies()
        {
            var radiographies = await _context.Radiographyies.ToListAsync();

            if (radiographies == null || !radiographies.Any())
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No radiography records found."
                });
            }

            var result = radiographies.Select(r => new
            {
                r.Id,
                r.First_Name,
                r.Middel_name,
                r.Last_Name,
                Age = r.Age.HasValue ? r.Age.Value.ToString("yyyy-MM-dd") : null,
                r.Consulting_reservationId,
                r.Price,
                ImageUrl = $"{Request.Scheme}://{Request.Host}/{r.ImagePath}"
            });

            return Ok(new
            {
                StatusCode = 200,
                Message = "All radiography records retrieved successfully.",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRadiographyById([FromRoute] long id)
        {
            var radiography = await _context.Radiographyies.FindAsync(id);

            if (radiography == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Radiography record not found."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Radiography record retrieved successfully.",
                Data = new
                {
                    radiography.Id,
                    radiography.First_Name,
                    radiography.Middel_name,
                    radiography.Last_Name,
                    Age = radiography.Age?.ToString("yyyy-MM-dd"),
                    radiography.Consulting_reservationId,
                    radiography.Price,
                    ImageUrl = $"{Request.Scheme}://{Request.Host}/{radiography.ImagePath}"
                }
            });
        }

        [HttpGet("radiographies/latest/{patientId}")]
        public async Task<IActionResult> GetLatestTwoRadiographies(long patientId)
        {
            var radiographies = await _context.Radiographyies
                .Where(r => r.Consulting_reservation.PatientId == patientId)
                .OrderByDescending(r => r.Id)
                .Take(2)
                .Select(r => new
                {
                    r.ImagePath,
                    ImageFileName = Path.GetFileName(r.ImagePath)
                })
                .ToListAsync();

            if (radiographies.Count == 0)
                return NotFound(new { message = "No radiography images found for this patient." });

            return Ok(new
            {
                message = "Latest two radiography images retrieved successfully.",
                data = radiographies
            });
        }



    }



}
