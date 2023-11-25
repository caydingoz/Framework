namespace Framework.Shared.Dtos.AuthServer.UserService
{
    public class LoginOutput
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public TokenOutput? Token { get; set; }
        public GetUserRolesAndPermissionsOutput? RolesAndPermissions { get; set; }
    }
}
