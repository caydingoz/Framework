using Framework.AuthServer.Models;
using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.ChatService.Output
{
    public class GetChatMessagesOutput
    {
        public ICollection<ChatMessage> ChatMessages { get; set; } = [];
    }
}
