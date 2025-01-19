namespace Framework.AuthServer.Dtos.ChatService.Input
{
    public class ReadChatMessageInput
    {
        public ICollection<int> ChatMessageIds { get; set; } = [];
    }
}
