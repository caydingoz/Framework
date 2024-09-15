namespace Framework.AuthServer.Dtos.RoleService.Input
{
    public class AddPermissionToRoleInput
    {
        public required ICollection<PermissionInput> Permissions { get; set; }
    }
}
