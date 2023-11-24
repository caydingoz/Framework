namespace Framework.Shared.Dtos.AuthServer
{
    public class LoginOutput
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public TokenOutput? Token { get; set; }
        public GetRolesAndPermissionsOutput? RolesAndPermissions { get; set; }
    }
}
