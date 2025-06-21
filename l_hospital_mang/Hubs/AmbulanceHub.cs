using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace l_hospital_mang.Hubs
{
    public class AmbulanceHub : Hub
    {
        private readonly ILogger<AmbulanceHub> _logger;

        public AmbulanceHub(ILogger<AmbulanceHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogWarning("ConnectionId is null or empty on OnConnectedAsync.");
                return;
            }

            var httpContext = Context.GetHttpContext();
            var role = httpContext?.Request.Query["role"].ToString();

            _logger.LogInformation($"New connection: {connectionId}, role: {role}");

            if (role == "Driver")
            {
                await Groups.AddToGroupAsync(connectionId, "Drivers");
                _logger.LogInformation($"Connection {connectionId} added to group 'Drivers'.");
            }

            await base.OnConnectedAsync();
        }

        // إشعار جميع السائقين بطلب إسعاف جديد
        public async Task NotifyDrivers(object requestInfo)
        {
            _logger.LogInformation("Sending ambulance request notification to Drivers group.");
            await Clients.Group("Drivers").SendAsync("ReceiveAmbulanceRequest", requestInfo);
        }

        // إشعار مريض معين بقبول الطلب من قبل سائق
        public async Task NotifyPatient(string patientUserId, object data)
        {
            if (string.IsNullOrEmpty(patientUserId))
            {
                _logger.LogWarning("NotifyPatient called with empty patientUserId.");
                return;
            }

            _logger.LogInformation($"Sending ambulance acceptance notification to patient user {patientUserId}.");
            await Clients.User(patientUserId).SendAsync("ReceiveAmbulanceResponse", data);
        }
    }
}
