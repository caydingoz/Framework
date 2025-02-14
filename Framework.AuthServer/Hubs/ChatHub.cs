using Framework.AuthServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace Framework.AuthServer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ConnectedUserService _connectedUserService;

        public ChatHub(ConnectedUserService connectedUserService)
        {
            _connectedUserService = connectedUserService;
        }
        public async override Task OnConnectedAsync()
        {
            string? userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _connectedUserService.AddUser(userId);
                await Clients.All.SendAsync("OnlineUsersUpdated", _connectedUserService.GetConnectedUsers());
            }
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            string? userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _connectedUserService.RemoveUser(userId);
                await Clients.All.SendAsync("OnlineUsersUpdated", _connectedUserService.GetConnectedUsers());
            }
        }
        public async Task SendMessage(string sender, string receiver, string message)
        {
            await Clients.User(receiver).SendAsync("ReceiveMessage", sender, message);
        }
    }

}
