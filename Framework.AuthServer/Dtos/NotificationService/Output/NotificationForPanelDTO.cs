using Framework.AuthServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.NotificationService.Output
{
    public class NotificationForPanelDTO
    {
        public int Id { get; set; }
        [StringLength(300)]
        public required string Title { get; set; }
        [StringLength(500)]
        public required string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public IDictionary<int, string> Roles { get; set; }
        public bool IsDeleted { get; set; }
    }
}
