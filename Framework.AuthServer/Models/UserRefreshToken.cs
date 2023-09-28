using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;

namespace Framework.AuthServer.Models
{
    public class UserRefreshToken : Entity<int>, ICachable
    {
        public Guid UserId { get; set; }
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
