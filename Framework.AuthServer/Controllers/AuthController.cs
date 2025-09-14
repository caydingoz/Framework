using Framework.Application;
using Framework.AuthServer.Dtos.AuthService.Input;
using Framework.AuthServer.Dtos.AuthService.Output;
using Framework.AuthServer.Enums;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Interfaces.Services;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Framework.Shared.Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<AuthController> Logger;

        private readonly ITokenHandlerService TokenHandlerService;
        private readonly IGenericRepository<User, Guid> UserRepository;
        private readonly IUserTokenRepository UserTokenRepository;

        public AuthController(
            Configuration configuration,
            ILogger<AuthController> logger,
            ITokenHandlerService tokenHandlerService,
            IGenericRepository<User, Guid> userRepository,
            IUserTokenRepository userTokenRepository
        )
        {
            Configuration = configuration;
            Logger = logger;
            TokenHandlerService = tokenHandlerService;
            UserRepository = userRepository;
            UserTokenRepository = userTokenRepository;
        }

        [HttpPost("login")]
        public async Task<GeneralResponse<LoginOutput>> LoginAsync(EmailLoginInput input)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                if (input is null)
                    throw new Exception("Invalid client request! (input null)");

                var user = await UserRepository.SingleOrDefaultAsync(x => 
                                                    x.Email == input.Email && x.Password == input.Password, 
                                                    includes: x => x.Roles.Select(y => y.Permissions)
                                                ) ?? throw new Exception("Not found a user with given email and password!");
                
                var permissions = user.Roles.SelectMany(role => role.Permissions).Distinct();

                var token = TokenHandlerService.CreateToken(user, permissions);

                var refreshTokenExpiryTime = (Configuration.JWT is null || Configuration.JWT.RefreshTokenValidityInDays == 0) ? 1 : Configuration.JWT.RefreshTokenValidityInDays;

                var newRefreshToken = new UserToken
                {
                    RefreshToken = token.RefreshToken,
                    AccessToken = token.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryTime), //TODO: Default Config
                    UserId = user.Id,
                };

                await UserTokenRepository.UpsertTokenAsync(user.Id, newRefreshToken);

                var res = new LoginOutput
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    Roles = user.Roles.Select(x => x.Name),
                    Permissions = permissions.ToDictionary(permission => permission.Operation.ToString(), permission => permission.Type)
                };

                return res;
            });
        }

        [HttpPost("signup")]
        public async Task<GeneralResponse<RegisterOutput>> SignUpAsync(EmailRegisterInput input)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                if (input is null)
                    throw new Exception("Invalid client request! (Input null)");

                if (await UserRepository.AnyAsync(x => x.Email == input.Email))
                    throw new Exception("Email already exist!");

                var user = new User
                {
                    Email = input.Email,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    PhoneNumber = input.PhoneNumber,
                    Password = input.Password,
                    Title = input.Title,
                    Status = UserStatus.Passive
                };

                await UserRepository.InsertOneAsync(user);

                var token = TokenHandlerService.CreateToken(user, Enumerable.Empty<Permission>());

                var refreshTokenExpiryTime = (Configuration.JWT is null || Configuration.JWT.RefreshTokenValidityInDays == 0) ? 1 : Configuration.JWT.RefreshTokenValidityInDays;

                await UserTokenRepository.InsertOneAsync(new UserToken
                {
                    RefreshToken = token.RefreshToken,
                    AccessToken = token.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryTime), //TODO: Default Config
                    UserId = user.Id
                });

                var res = new RegisterOutput
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    Roles = [],
                    Permissions = []
                };

                return res;
            });
        }

        [HttpPost("refresh-token")]
        public async Task<GeneralResponse<TokenOutput>> RefreshTokenAsync(RefreshTokenInput input)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                if (input is null)
                    throw new Exception("Invalid client request! (input null)");

                var userId = TokenHandlerService.GetUserIdFromToken(input.AccessToken);

                var user = await UserRepository.GetByIdAsync(userId, includes: x => x.Roles.Select(y => y.Permissions)) ?? throw new Exception("Invalid access token or refresh token");
                if (user is null)
                    throw new Exception("Invalid access token or refresh token");

                var isValid = await UserTokenRepository.AnyAsync(x => x.UserId == userId && x.RefreshToken == input.RefreshToken && x.RefreshTokenExpiryTime >= DateTime.UtcNow);

                if (!isValid)
                    throw new Exception("Invalid access token or refresh token");

                var permissions = user.Roles.SelectMany(role => role.Permissions).Distinct();

                var token = TokenHandlerService.CreateToken(user, permissions);

                var refreshTokenExpiryTime = (Configuration.JWT is null || Configuration.JWT.RefreshTokenValidityInDays == 0) ? 1 : Configuration.JWT.RefreshTokenValidityInDays;

                var newRefreshToken = new UserToken
                {
                    RefreshToken = token.RefreshToken,
                    AccessToken = token.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryTime), //TODO: Default Config
                    UserId = user.Id
                };

                await UserTokenRepository.UpsertTokenAsync(user.Id, newRefreshToken);

                return token;
            });
        }

        [HttpGet("roles-and-permissions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<GeneralResponse<GetUserRolesAndPermissionsOutput>> GetUserRolesAndPermissionsAsync()
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();

                var user = await UserRepository.GetByIdAsync(userId, includes: x => x.Roles.Select(y => y.Permissions)) ?? throw new Exception("Not found a user!");

                var permissions = user.Roles.SelectMany(role => role.Permissions).Distinct();

                var res = new GetUserRolesAndPermissionsOutput
                {
                    Roles = user.Roles.Select(x => x.Name),
                    Permissions = permissions.ToDictionary(permission => permission.Operation.ToString(), permission => permission.Type)
                };

                return res;
            });
        }
    }
}