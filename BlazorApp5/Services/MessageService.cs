using BlazorApp5.Data;
using BlazorApp5.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp5.Services
{
    public class MessageService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public MessageService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Message>> GetMessagesAsync(string userCode, string partnerCode)
        {
            await using var db = _dbFactory.CreateDbContext();

            return await db.Messages
                .Where(m =>
                    (m.SenderCode == userCode && m.ReceiverCode == partnerCode) ||
                    (m.SenderCode == partnerCode && m.ReceiverCode == userCode))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }

}
