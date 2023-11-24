using Framework.Shared.Enums;

namespace Framework.Shared.Dtos.AuthServer
{
    public class GetRolesAndPermissionsOutput
    {
        public IEnumerable<string>? Roles { get; set; }
        public Dictionary<string, Permissions>? Permissions { get; set; }
    }
}
