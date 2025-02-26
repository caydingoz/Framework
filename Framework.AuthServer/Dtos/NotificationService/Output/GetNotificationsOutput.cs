using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.NotificationService.Output
{
    public class GetNotificationsOutput : PageOutput
    {
        public ICollection<NotificationDTO> Notifications { get; set; } = [];
        public long UnreadCount { get; set; }
    }
}
