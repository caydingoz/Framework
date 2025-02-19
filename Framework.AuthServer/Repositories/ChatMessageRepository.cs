using Framework.AuthServer.Dtos.ChatService.Output;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer.Repositories
{
    public class ChatMessageRepository : EfCoreRepositoryBase<ChatMessage, AuthServerDbContext, int>, IChatMessageRepository
    {
        public ChatMessageRepository(AuthServerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<GetChatOverviewOutput> GetChatOverviewAsync(Guid userId, int page, int count)
        {
            var chatMessages = await DbContext.ChatMessages
                .AsNoTracking()
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .GroupBy(x => x.SenderId == userId ? x.ReceiverId : x.SenderId)
                .Select(g => new
                {
                    LastMessage = g.OrderByDescending(x => x.SentAt).First(),
                    UnReadMessageCount = g.Count(x => x.ReceiverId == userId && !x.IsRead)
                })
                .OrderByDescending(x => x.UnReadMessageCount)
                .Skip(page * count)
                .Take(count)
                .ToListAsync();

            var res = new GetChatOverviewOutput();

            foreach (var chatMessage in chatMessages.OrderByDescending(x => x.LastMessage.SentAt))
            {
                if(chatMessage is not null)
                    res.Chats.Add(new ChatOverviewOutput
                    {
                        UserId = chatMessage.LastMessage.SenderId == userId ? chatMessage.LastMessage.ReceiverId : chatMessage.LastMessage.SenderId,
                        LastMessage = chatMessage.LastMessage.Content,
                        UnReadMessageCount = chatMessage.UnReadMessageCount
                    });
            }

            return res;
        }
    }
}
