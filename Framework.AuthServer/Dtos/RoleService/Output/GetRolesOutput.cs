using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.RoleService.Output
{
    public class GetRolesOutput : PageOutput
    {
        public ICollection<RolesOutput> Roles { get; set; } = new List<RolesOutput>();
    }
    public class RolesOutput
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
