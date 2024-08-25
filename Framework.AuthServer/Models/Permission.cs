using Framework.AuthServer.Enums;
using Framework.Domain.Entites;
using Framework.Shared.Enums;

namespace Framework.AuthServer.Models
{
    public class Permission : Entity<int>
    {
        public required Operations Operation { get; set; }
        public PermissionTypes Type { get; set; }
    }
}
