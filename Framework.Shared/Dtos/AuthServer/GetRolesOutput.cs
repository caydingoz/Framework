namespace Framework.Shared.Dtos.AuthServer
{
    public class GetRolesOutput
    {
        public ICollection<string> Roles { get; set; } = new List<string>();
    }
}
