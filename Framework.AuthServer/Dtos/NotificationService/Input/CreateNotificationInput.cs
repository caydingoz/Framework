using Framework.AuthServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.NotificationService.Input
{
    public class CreateNotificationInput
    {
        [StringLength(300)]
        public required string Title { get; set; }
        [StringLength(500)]
        public required string Message { get; set; }
        public NotificationTypes Type { get; set; }
        public ICollection<Guid> UserIds { get; set; }
    }
}
