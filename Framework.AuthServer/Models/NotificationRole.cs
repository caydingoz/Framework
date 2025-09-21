using Framework.Domain.Entites;

namespace Framework.AuthServer.Models
{
    public class NotificationRole : Entity<int>
    {
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }

}
