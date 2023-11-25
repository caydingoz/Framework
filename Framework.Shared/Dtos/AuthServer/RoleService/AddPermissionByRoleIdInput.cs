using Framework.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.Shared.Dtos.AuthServer.RoleService
{
    public class AddPermissionByRoleIdInput
    {
        public required string Operation { get; set; }
        public PermissionTypes PermissionType { get; set; }
    }
}
