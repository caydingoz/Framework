using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class UserRefreshToken : Entity<int>, ICachable //TODO: remove icachable later
    {
        [StringLength(450)]
        public required string UserId { get; set; }
        [StringLength(500)]
        public required string RefreshToken { get; set; }
        [StringLength(1000)]
        public required string AccessToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
