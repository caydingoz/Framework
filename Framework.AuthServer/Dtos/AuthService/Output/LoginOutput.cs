using Framework.Shared.Enums;

namespace Framework.AuthServer.Dtos.AuthService.Output
{
    public class LoginOutput
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public TokenOutput? Token { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public Dictionary<string, PermissionTypes> Permissions { get; set; } = [];
    }
}
