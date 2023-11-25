using Framework.AuthServer.Models;
using Framework.Shared.Dtos.AuthServer.UserService;
using Framework.Shared.Enums;
using System.Security.Claims;

namespace Framework.AuthServer.Interfaces.Services
{
    public interface ITokenHandlerService
    {
        public TokenOutput CreateToken(User user, Dictionary<string, PermissionTypes> permissions);
        public string GenerateRefreshToken();
        public IEnumerable<Claim> GetPrincipalFromExpiredToken(string token);
    }
}
