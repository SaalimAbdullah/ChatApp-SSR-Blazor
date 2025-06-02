using BlazorApp5.Data;
using BlazorApp5.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp5.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public ChatHub(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

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
            await using var db = _dbFactory.CreateDbContext();

            db.Messages.Add(new Message
            {
                SenderCode = senderCode,
                ReceiverCode = receiverCode,
                MessageText = message,
                SentAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            var receiverUser = await db.Users.FirstOrDefaultAsync(u => u.UserCode == receiverCode);
            var senderUser = await db.Users.FirstOrDefaultAsync(u => u.UserCode == senderCode);

            if (receiverUser != null && senderUser != null)
            {
                var alreadyAdded = await db.Contacts.AnyAsync(c =>
                    c.OwnerUserId == receiverUser.Id && c.ContactUserCode == senderCode);

                if (!alreadyAdded)
                {
                    db.Contacts.Add(new Contact
                    {
                        OwnerUserId = receiverUser.Id,
                        ContactUserCode = senderCode,
                        ContactDisplayName = senderCode // or use senderUser.Name if available
                    });

                    await db.SaveChangesAsync();

                    if (UserConnections.TryGetValue(receiverCode, out var contactAddedConnId))
                    {
                        await Clients.Client(contactAddedConnId)
                            .SendAsync("ContactAdded", senderCode, senderCode);
                    }
                }
            }


            // SignalR delivery
            if (UserConnections.TryGetValue(receiverCode, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderCode, message);
            }

            if (UserConnections.TryGetValue(senderCode, out var senderConnectionId))
            {
                await Clients.Client(senderConnectionId).SendAsync("ReceiveMessage", senderCode, message);
            }
        }

    }
}