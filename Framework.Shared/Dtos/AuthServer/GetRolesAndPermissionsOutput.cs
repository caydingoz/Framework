namespace Framework.Shared.Dtos.AuthServer
{
    public class GetRolesAndPermissionsOutput
    {
        public IEnumerable<string>? Roles { get; set; }
        public Dictionary<string, short>? Permissions { get; set; }
    }
}
