using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace l_hospital_mang.Hubs
{
    public class DelayNotificationHub : Hub
    {
        private readonly ILogger<DelayNotificationHub> _logger;

        public DelayNotificationHub(ILogger<DelayNotificationHub> logger)
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

            var userId = Context.UserIdentifier;
            _logger.LogInformation($" Connected: {connectionId}, UserId: {userId}");

            await Groups.AddToGroupAsync(connectionId, "Patients");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation($" Disconnected: {connectionId}");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
