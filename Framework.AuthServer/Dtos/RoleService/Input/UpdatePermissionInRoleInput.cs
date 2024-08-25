namespace Framework.AuthServer.Dtos.RoleService.Input
{
    public class UpdatePermissionInRoleInput
    {
        public required ICollection<PermissionInput> Permissions { get; set; }
    }
}
