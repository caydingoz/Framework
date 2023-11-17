namespace Framework.Shared.Dtos.AuthServer
{
    public class GetRolesOutput
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public ICollection<string> Roles { get; set; } = new List<string>();
    }
}
