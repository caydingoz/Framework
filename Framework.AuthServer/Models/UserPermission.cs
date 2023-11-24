using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using Framework.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class UserPermission : Entity<int>, ICachable
    {
        [StringLength(100)]
        public required string RoleId { get; set; }
        [StringLength(100)]
        public required string Operation { get; set; }
        public Permissions Permissions { get; set; }
    }
}
