using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using l_hospital_mang.Data;
using l_hospital_mang.Data.Models;
using l_hospital_mang.DTOs;
using l_hospital_mang.Hubs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AmbulanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<AmbulanceHub> _hubContext;
        private readonly UserManager<IdentityUser> _userManager;

        public AmbulanceController(AppDbContext context, IHubContext<AmbulanceHub> hubContext, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("request")]
        public async Task<IActionResult> RequestAmbulance([FromForm] AmbulanceRequestDto dto)
        {
            var patientIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(patientIdClaim) || !long.TryParse(patientIdClaim, out long patientId))
                return Unauthorized(new { message = "Invalid or missing patient ID in token." });

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            var request = new AmbulanceRequest
            {
                PatientId = patientId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = "Pending",
                RequestTime = DateTime.UtcNow
            };

            _context.AmbulanceRequests.Add(request);
            await _context.SaveChangesAsync();

            var patientName = $"{patient.First_Name} {patient.Middel_name} {patient.Last_Name}";

            await _hubContext.Clients.Group("Drivers").SendAsync("ReceiveAmbulanceRequest", new
            {
                requestId = request.Id,
                latitude = request.Latitude,
                longitude = request.Longitude,
                patientName
            });

            return Ok(new
            {
                status = 200,
                message = "The request has been sent, awaiting drivers' response",
                requestId = request.Id,
                latitude = request.Latitude,
                longitude = request.Longitude
            });
        }


        [Authorize(Roles = "Driver")]
        [HttpPost("accept/{requestId}")]
        public async Task<IActionResult> AcceptRequest(long requestId)
        {
            var employeeIdClaim = User.FindFirst("userId")?.Value;
            if (!long.TryParse(employeeIdClaim, out long employeeId))
                return Unauthorized(new { message = "Unauthorized access." });

            var request = await _context.AmbulanceRequests
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return NotFound(new { message = "Request not found." });

            if (request.Status != "Pending")
            {
                return BadRequest(new { message = "This request has already been accepted by another driver." });
            }

            var car = await _context.CAmbulance_Car.FirstOrDefaultAsync(c => c.IsAvailable);
            if (car == null)
            {
                return BadRequest(new { message = "No ambulance car is available." });
            }

            var driver = await _context.Employeess.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (driver == null)
                return Unauthorized(new { message = "Driver not found." });

            request.Status = "Accepted";
            request.AcceptedByEmployeeId = employeeId;
            request.CarId = car.Id;
            car.IsAvailable = false;

            await _context.SaveChangesAsync();

            var driverFullName = $"{driver.First_Name} {driver.Middel_name} {driver.Last_Name}";


            return Ok(new
            {
                message = "Request accepted successfully.",
                request.Id,
                car.CarNumber,
                driverName = driverFullName
            });
        }



        [Authorize(Roles = "Driver")]
        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var pendingRequests = await _context.AmbulanceRequests
                .Where(r => r.Status == "Pending")
                .Select(r => new
                {
                    r.Id,
                    r.Latitude,
                    r.Longitude,
                    r.RequestTime
                }).ToListAsync();

            return Ok(pendingRequests);
        }

        [Authorize(Roles = "Driver")]
        [HttpGet("location/{requestId}")]
        public async Task<IActionResult> GetPatientLocation(long requestId)
        {
            var request = await _context.AmbulanceRequests.FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
                return NotFound(new { status = 404, message = "Request not found" });

            return Ok(new
            {
                status = 200,
                latitude = request.Latitude,
                longitude = request.Longitude
            });
        }
    }
}
