using Framework.Domain.Entites;

namespace Framework.AuthServer.Models
{
    public class NotificationUser : Entity<int>
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }
        public bool IsRead { get; set; } = false;
    }

}
