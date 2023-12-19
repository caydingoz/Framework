using Framework.Domain.Entites;
using Framework.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class Permission : Entity<int>
    {
        [StringLength(450)]
        public required string RoleId { get; set; }
        [StringLength(100)]
        public required string Operation { get; set; }
        public PermissionTypes Type { get; set; }
        public Role? Role { get; set; }
    }
}
