using Framework.AuthServer.Enums;
using System.Text.Json.Serialization;

namespace Framework.AuthServer.Dtos.RoleService.Input
{
    public class RemovePermissionFromRoleInput
    {
        public required int[] PermissionIds { get; set; }
    }
}
