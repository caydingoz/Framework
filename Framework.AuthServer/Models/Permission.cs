using Framework.AuthServer.Enums;
using Framework.Domain.Entites;
using Framework.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class Permission : Entity<int>
    {
        [StringLength(150)]
        public required string Operation { get; set; }
        public PermissionTypes Type { get; set; }
    }
}
