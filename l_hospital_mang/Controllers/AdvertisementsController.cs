using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using l_hospital_mang.DTOs;
using l_hospital_mang.DTOs.l_hospital_mang.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisementsController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        public AdvertisementsController(IConfiguration configuration, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager,
        AppDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }
        [Authorize(Roles = "Doctor,Manager")]
        [HttpPost("create-advertisement/{clinicId}")]
        public async Task<IActionResult> CreateAdvertisement(long clinicId, [FromForm] AdvertisementServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clinic = await _context.Clinicscss.FindAsync(clinicId);
            if (clinic == null)
                return NotFound(new { message = "Clinic not found." });

            var existingAd = await _context.Advertismentss
                .FirstOrDefaultAsync(a => a.ClinicId == clinicId);

            if (existingAd != null)
            {
                return Conflict(new
                {
                    message = "An advertisement already exists for this clinic.",
                });
            }

            var ad = new Advertisments
            {
                ServiceName = dto.ServiceName,
                ClinicId = clinicId,
                DiscountDegree = dto.DiscountDegree
            };

            _context.Advertismentss.Add(ad);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Advertisement created successfully.",
                advertisement = new
                {
                    ad.Id,
                    ad.ServiceName,
                    ad.DiscountDegree,
                    Clinic = new
                    {
                        clinic.Id,
                        clinic.Clinic_Name
                    }
                }
            });
        }
        [Authorize(Roles = "Doctor,Manager")]
        [HttpPut("update-advertisement/{advertisementId}")]
        public async Task<IActionResult> UpdateAdvertisement(long advertisementId, [FromForm] AdvertisementUpdateDto dto)
        {
            var ad = await _context.Advertismentss
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == advertisementId);

            if (ad == null)
                return NotFound(new { message = "Advertisement not found." });

            if (!string.IsNullOrEmpty(dto.ServiceName))
                ad.ServiceName = dto.ServiceName;

            if (dto.DiscountDegree.HasValue)
                ad.DiscountDegree = dto.DiscountDegree.Value;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Advertisement updated successfully.",
                advertisement = new
                {
                    ad.Id,
                    ad.ServiceName,
                    ad.DiscountDegree,
                    Clinic = new
                    {
                        ad.Clinic?.Id,
                        ad.Clinic?.Clinic_Name
                    }
                }
            });
        }
        [Authorize(Roles = "Doctor,Manager")]
        [HttpDelete("delete-advertisement/{id}")]
        public async Task<IActionResult> DeleteAdvertisement(long id)
        {
            var ad = await _context.Advertismentss.FindAsync(id);
            if (ad == null)
                return NotFound(new { message = "Advertisement not found." });

            _context.Advertismentss.Remove(ad);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Advertisement deleted successfully." });
        }
        [Authorize(Roles = "Doctor,Manager")]
        [HttpGet("advertisement/{id}")]
        public async Task<IActionResult> GetAdvertisement(long id)
        {
            var ad = await _context.Advertismentss
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad == null)
                return NotFound(new { message = "Advertisement not found." });

            return Ok(new
            {
                ad.Id,
                ad.ServiceName,
                ad.DiscountDegree,
                Clinic = new
                {
                    ad.Clinic.Id,
                    ad.Clinic.Clinic_Name
                }
            });
        }
        [Authorize(Roles = "Doctor,Manager")]
        [HttpGet("all-advertisements")]
        public async Task<IActionResult> GetAllAdvertisements()
        {
            var ads = await _context.Advertismentss
                .Include(a => a.Clinic)
                .Select(ad => new
                {
                    ad.Id,
                    ad.ServiceName,
                    ad.DiscountDegree,
                    Clinic = new
                    {
                        ad.Clinic.Id,
                        ad.Clinic.Clinic_Name
                    }
                })
                .ToListAsync();

            if (ads == null || !ads.Any())
            {
                return NotFound(new { message = "There are no advertisements to display." });
            }

            return Ok(ads);
        }

    }
}
