using Framework.Shared.Enums;

namespace Framework.AuthServer.Dtos.AuthService.Output
{
    public class GetUserRolesAndPermissionsOutput
    {
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public Dictionary<string, PermissionTypes> Permissions { get; set; } = new Dictionary<string, PermissionTypes>();
    }
}
