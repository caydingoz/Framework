using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class UserRefreshToken : Entity<int>, ICachable
    {
        public Guid UserId { get; set; }
        [StringLength(500)]
        public string? RefreshToken { get; set; }
        [StringLength(1000)]
        public string? AccessToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
