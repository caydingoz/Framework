using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class UserToken : Entity<int>
    {
        [StringLength(450)]
        public required Guid UserId { get; set; }
        [StringLength(500)]
        public required string RefreshToken { get; set; }
        [StringLength(1000)]
        public required string AccessToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public User? User { get; set; }
    }
}
