using Framework.Domain.Entites;

namespace Framework.AuthServer.Models
{
    public class Role : Entity<int>
    {
        public required string Name { get; set; }
        public ICollection<Permission> Permissions { get; set; } = [];
        public ICollection<User> Users { get; set; } = [];
    }
}
