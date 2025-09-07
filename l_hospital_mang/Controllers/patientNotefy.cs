using l_hospital_mang.Data;
using l_hospital_mang.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class patientNotefy : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<DelayNotificationHub> _hubContext;
        private readonly ILogger<patientNotefy> _logger;

        public patientNotefy(AppDbContext context, IHubContext<DelayNotificationHub> hubContext, ILogger<patientNotefy> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        [Authorize(Roles = "Doctor,Manager,LabDoctor,RadiographyDoctor")]
        [HttpPost("notify-patients-delay/{dateId}")]
        public async Task<IActionResult> NotifyPatientsOfDelay([FromRoute] long dateId, [FromForm] int delayMinutes)
        {
            if (delayMinutes <= 0)
            {
                _logger.LogWarning("Invalid delayMinutes value: {DelayMinutes}", delayMinutes);
                return BadRequest(new { status = 400, message = "Delay minutes must be greater than zero." });
            }

            var date = await _context.Datess.FindAsync(dateId);
            if (date == null)
            {
                _logger.LogWarning("Date not found for dateId: {DateId}", dateId);
                return NotFound(new { status = 404, message = "Date not found." });
            }

            var reservations = await _context.Consulting_reservations
                .Where(r => r.DateId == dateId)
                .Include(r => r.Patient)
                .ToListAsync();

            if (!reservations.Any())
            {
                _logger.LogWarning("No reservations found for dateId: {DateId}", dateId);
                return NotFound(new { status = 404, message = "No reservations found for this date." });
            }

            var doctor = await _context.Doctorss.FindAsync(date.DoctorId);
            var doctorName = doctor != null
                ? $"{doctor.First_Name} {doctor.Middel_name} {doctor.Last_Name}".Trim()
                : "الطبيب";

            _logger.LogInformation("Sending delay notifications for dateId: {DateId} with delayMinutes: {DelayMinutes}. Total reservations: {Count}", dateId, delayMinutes, reservations.Count);

            foreach (var reservation in reservations)
            {
                var patient = reservation.Patient;
                var id = patient.Id;

                if (id > 0)
                {
                    var userId = id.ToString();
                    var message = $"نأسف، سيتم تأخير موعدك مع الدكتور {doctorName} بمقدار {delayMinutes} دقيقة.";
                    _logger.LogInformation("Sending notification to UserId: {UserId} - Message: {Message}", userId, message);

                    await _hubContext.Clients.User(userId).SendAsync("ReceiveDelayNotification", new
                    {
                        delayMinutes,
                        doctorName,
                        message
                    });

                    _logger.LogInformation("Notification sent successfully to UserId: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Patient with reservationId: {ReservationId} has invalid Id", reservation.Id);
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