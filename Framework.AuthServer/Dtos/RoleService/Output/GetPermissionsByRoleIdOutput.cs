﻿using Framework.Shared.Dtos;
using Framework.Shared.Enums;

namespace Framework.AuthServer.Dtos.RoleService.Output
{
    public class GetPermissionsByRoleIdOutput : PageOutput
    {
        public required int RoleId { get; set; }
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
