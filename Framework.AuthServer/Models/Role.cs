using Framework.Domain.Interfaces.Entities;
using Microsoft.AspNetCore.Identity;

namespace Framework.AuthServer.Models
{
    public class Role : IdentityRole<string>, IUpdated, ICreated
    {
        public ICollection<Permission>? Permissions { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
