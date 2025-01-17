using Framework.AuthServer.Dtos.NotificationService.Output;

namespace Framework.AuthServer.Dtos.NotificationService.Input
{
    public class CreateNotificationInput
    {
        public ICollection<NotificationDTO> Notifications { get; set; } = [];
    }
}
