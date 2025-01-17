using Framework.AuthServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.NotificationService.Output
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        [StringLength(300)]
        public required string Title { get; set; }
        [StringLength(500)]
        public required string Message { get; set; }
        public NotificationTypes Type { get; set; }
        public bool IsRead { get; set; }
        public Guid UserId { get; set; }
    }

}
