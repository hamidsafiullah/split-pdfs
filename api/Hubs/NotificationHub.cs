using Microsoft.AspNetCore.SignalR;

namespace api.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("Receive", message);
        }
    }
}
