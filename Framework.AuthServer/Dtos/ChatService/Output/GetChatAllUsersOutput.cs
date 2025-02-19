namespace Framework.AuthServer.Dtos.ChatService.Output
{
    public class GetChatAllUsersOutput
    {
        public ICollection<ChatUserOutput> Users { get; set; } = [];
    }
    public class ChatUserOutput
    {
        public string Name { get; set; } = "";
        public string? Image { get; set; }
        public Guid UserId { get; set; }
    }
}
