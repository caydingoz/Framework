namespace Framework.AuthServer.Dtos.ChatService.Output
{
    public class GetChatOverviewOutput
    {
        public ICollection<ChatOverviewOutput> Chats { get; set; } = [];
    }
    public class ChatOverviewOutput
    {
        public string Name { get; set; } = "";
        public string? Image { get; set; }
        public Guid UserId { get; set; }
        public required string LastMessage { get; set; }
        public required DateTime LastMessageTime { get; set; }
        public int UnReadMessageCount { get; set; }
    }
}
