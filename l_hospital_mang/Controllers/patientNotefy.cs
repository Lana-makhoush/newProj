using l_hospital_mang.Data;
using l_hospital_mang.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class patientNotefy : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<DelayNotificationHub> _hubContext;

        public patientNotefy(AppDbContext context, IHubContext<DelayNotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]
        [HttpPost("notify-patients-delay/{dateId}")]
        public async Task<IActionResult> NotifyPatientsOfDelay([FromRoute] long dateId, [FromForm] int delayMinutes)
        {
            if (delayMinutes <= 0)
            {
                return BadRequest(new { status = 400, message = "Delay minutes must be greater than zero." });
            }

            var date = await _context.Datess.FindAsync(dateId);
            if (date == null)
            {
                return NotFound(new { status = 404, message = "Date not found." });
            }

            var reservations = await _context.Consulting_reservations
                .Where(r => r.DateId == dateId)
                .Include(r => r.Patient)
                .ToListAsync();

            if (!reservations.Any())
            {
                return NotFound(new { status = 404, message = "No reservations found for this date." });
            }

            var doctor = await _context.Doctorss.FindAsync(date.DoctorId);
            var doctorName = doctor != null
                ? $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}".Trim()
                : "الطبيب";

            foreach (var reservation in reservations)
            {
                var patient = reservation.Patient;
                var userId = patient.IdentityUserId;

                if (!string.IsNullOrEmpty(userId))
                {
                    var message = $"نأسف، سيتم تأخير موعدك مع الدكتور {doctorName} بمقدار {delayMinutes} دقيقة.";

                    await _hubContext.Clients.User(userId).SendAsync("ReceiveDelayNotification", new
                    {
                        delayMinutes,
                        doctorName,
                        message
                    });
                }
            }

            return Ok(new
            {
                status = 200,
                message = "Delay notifications sent successfully.",
                totalPatients = reservations.Count
            });
        }
    }
}
