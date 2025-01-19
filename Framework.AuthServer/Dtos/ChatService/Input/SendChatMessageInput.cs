namespace Framework.AuthServer.Dtos.ChatService.Input
{
    public class SendChatMessageInput
    {
        public required string Content { get; set; }
        public Guid ReceiverId { get; set; }
    }
}
