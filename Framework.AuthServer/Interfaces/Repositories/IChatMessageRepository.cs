using Framework.AuthServer.Dtos.ChatService.Output;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;

namespace Framework.AuthServer.Interfaces.Repositories
{
    public interface IChatMessageRepository : IGenericRepository<ChatMessage, int>
    {
        Task<GetChatOverviewOutput> GetChatOverviewAsync(Guid userId, int page, int count);
    }
}
