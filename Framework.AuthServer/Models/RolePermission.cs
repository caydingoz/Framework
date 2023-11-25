using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using Framework.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class RolePermission : Entity<int>, ICachable
    {
        [StringLength(450)]
        public required string RoleId { get; set; }
        [StringLength(100)]
        public required string Operation { get; set; }
        public PermissionTypes PermissionType { get; set; }
        public IdentityRole? Role { get; set; }
    }
}
