namespace Framework.Shared.Dtos.AuthServer.RoleService
{
    public class GetRolesOutput : PageOutput
    {
        public ICollection<RolesOutput> Roles { get; set; } = new List<RolesOutput>();
    }
    public class RolesOutput
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
