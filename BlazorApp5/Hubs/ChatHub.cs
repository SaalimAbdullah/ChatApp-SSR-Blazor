using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp5.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> UserConnections = new(); // code => connectionId

        public override Task OnConnectedAsync()
        {
            // Nothing yet - client will call RegisterCode
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var code = UserConnections.FirstOrDefault(x => x.Value == connectionId).Key;
            if (code != null)
            {
                UserConnections.Remove(code);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public Task RegisterCode(string code)
        {
            // Save user's 4-digit code with their connection ID
            UserConnections[code] = Context.ConnectionId;
            return Task.CompletedTask;
        }

        public async Task SendMessage(string senderCode, string receiverCode, string message)
        {
            if (UserConnections.TryGetValue(receiverCode, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderCode, message);
            }

            // Also send message back to sender (optional)
            if (UserConnections.TryGetValue(senderCode, out var senderConnectionId))
            {
                await Clients.Client(senderConnectionId).SendAsync("ReceiveMessage", senderCode, message);
            }
        }
    }
}