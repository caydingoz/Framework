using Framework.Shared.Enums;

namespace Framework.Shared.Dtos.AuthServer.RoleService
{
    public class UpdatePermissionByRoleIdInput
    {
        public required int Id { get; set; }
        public required PermissionTypes PermissionType { get; set; }
    }
}
