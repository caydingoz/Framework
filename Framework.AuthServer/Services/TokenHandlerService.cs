using Framework.AuthServer.Interfaces.Services;
using Framework.AuthServer.Models;
using Framework.Shared.Dtos.AuthServer;
using Framework.Shared.Entities.Configurations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Framework.AuthServer.Services
{
    public class TokenHandlerService : ITokenHandlerService
    {
        Configuration Configuration { get; }
        public TokenHandlerService(Configuration configuration)
        {
            Configuration = configuration;
        }
        public TokenOutput CreateToken(User user, ICollection<string> permissions)
        {
            JwtSecurityToken token;
            string refreshToken;
            (token, refreshToken) = GenerateAccessTokenAndRefreshToken(user, permissions);
            var tokenResult = new TokenOutput
            {
                ExpiresIn = (long)token.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds,
                RefreshToken = refreshToken,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            };
            return tokenResult;
        }

        private Tuple<JwtSecurityToken, string> GenerateAccessTokenAndRefreshToken(User user, ICollection<string> permissions)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("permissions", string.Join(",", permissions)),
            };

            return new Tuple<JwtSecurityToken, string>(GenerateAccessToken(authClaims), GenerateRefreshToken());
        }

        private JwtSecurityToken GenerateAccessToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWT.Secret));

            var token = new JwtSecurityToken(
                issuer: Configuration.JWT.ValidIssuer,
                audience: Configuration.JWT.ValidAudience,
                expires: DateTime.UtcNow.AddMinutes(Configuration.JWT.TokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public IEnumerable<Claim> GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            return jwtToken.Claims;
        }
    }
}
