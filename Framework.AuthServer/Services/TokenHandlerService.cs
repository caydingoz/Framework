using Framework.AuthServer.Dtos.AuthService.Output;
using Framework.AuthServer.Interfaces.Services;
using Framework.AuthServer.Models;
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
        public TokenOutput CreateToken(User user, IEnumerable<Permission> permissions)
        {
            var permissionList = new List<string>();

            foreach (var permission in permissions)
                permissionList.Add(permission.Operation + ":" + permission.Type);

            JwtSecurityToken token;
            string refreshToken;
            (token, refreshToken) = GenerateAccessTokenAndRefreshToken(user, permissionList);
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
                new(ClaimTypes.Name, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Email, user.Email),
                new("permissions", string.Join(";", permissions)),
            };

            return new Tuple<JwtSecurityToken, string>(GenerateAccessToken(authClaims), GenerateRefreshToken());
        }

        private JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> authClaims)
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

        public Guid GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims;

            if (claims is null || !claims.Any())
                throw new Exception("Invalid access token or refresh token!");

            var userId = claims.FirstOrDefault(m => m.Type == ClaimTypes.Name) ?? throw new Exception("Invalid access token or refresh token!");

            try
            {
                return new Guid(userId.Value);
            }
            catch (Exception)
            {
                throw new Exception("Invalid access token or refresh token! ID couldn't converted!");
            }
        }
    }
}
