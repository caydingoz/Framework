using Framework.AuthServer.Models;
using Framework.Shared.Dtos.AuthServer;
using System.Security.Claims;

namespace Framework.AuthServer.Interfaces.Services
{
    public interface ITokenHandlerService
    {
        public TokenOutput CreateToken(User user);
        public string GenerateRefreshToken();
        public IEnumerable<Claim> GetPrincipalFromExpiredToken(string token);
    }
}
