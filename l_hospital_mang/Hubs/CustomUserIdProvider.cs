using Microsoft.AspNetCore.SignalR;

namespace l_hospital_mang.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("userId")?.Value;
        }
    }

}
