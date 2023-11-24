using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class UserPermission : Entity<int>, ICachable
    {
        [StringLength(100)]
        public required string RoleId { get; set; }
        [StringLength(100)]
        public required string Operation { get; set; }
        public short Permissions { get; set; }
    }
}
