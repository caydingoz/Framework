using Framework.Shared.Enums;

namespace Framework.Shared.Dtos.AuthServer
{
    public class GetRolesAndPermissionsOutput
    {
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public Dictionary<string, PermissionTypes> Permissions { get; set; } = new Dictionary<string, PermissionTypes>();
    }
}
