using Framework.Shared.Enums;

namespace Framework.Shared.Dtos.AuthServer.RoleService
{
    public class GetRolesOutput
    {
        public ICollection<RolesOutput> Roles { get; set; } = new List<RolesOutput>();
    }
    public class RolesOutput
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
    }
}
