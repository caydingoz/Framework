using Framework.Shared.Enums;

namespace Framework.Shared.Dtos.AuthServer.RoleService
{
    public class GetPermissionsByRoleIdOutput
    {
        public required string RoleId { get; set; }
        public ICollection<PermissionsOutput> Permissions { get; set; } = new List<PermissionsOutput>();
    }
    public class PermissionsOutput
    {
        public required int Id { get; set; }
        public required string Operation { get; set; }
        public PermissionTypes Type { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
