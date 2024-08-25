using Framework.AuthServer.Dtos.AuthService.Output;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Interfaces.Services
{
    public interface ITokenHandlerService
    {
        public TokenOutput CreateToken(User user, IEnumerable<Permission> permissions);
        public string GenerateRefreshToken();
        public Guid GetUserIdFromToken(string token);
    }
}
