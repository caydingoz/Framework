using Framework.Application;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Interfaces.Services;
using Framework.AuthServer.Models;
using Framework.Shared.Dtos;
using Framework.Shared.Dtos.AuthServer.UserService;
using Framework.Shared.Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Framework.AuthServer.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<AuthController> Logger;

        private readonly SignInManager<User> SignInManager;
        private readonly UserManager<User> UserManager;
        private readonly IUserStore<User> UserStore;
        private readonly IUserEmailStore<User> EmailStore;
        private readonly ITokenHandlerService TokenHandlerService;
        private readonly IUserRefreshTokenRepository UserRefreshTokenRepository;
        private readonly IPermissionRepository RolePermissionRepository;

        public AuthController(
            Configuration configuration,
            ILogger<AuthController> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            ITokenHandlerService tokenHandlerService,
            IUserRefreshTokenRepository userRefreshTokenRepository,
            IPermissionRepository rolePermissionRepository
        )
        {
            Configuration = configuration;
            Logger = logger;
            SignInManager = signInManager;
            UserManager = userManager;
            UserStore = userStore;
            TokenHandlerService = tokenHandlerService;
            UserRefreshTokenRepository = userRefreshTokenRepository;
            RolePermissionRepository = rolePermissionRepository;
            EmailStore = GetEmailStore();
        }
        [HttpPost("login")]
        public async Task<GeneralResponse<LoginOutput>> LoginAsync(EmailLoginInput input)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                if (input is null)
                    throw new Exception("Invalid client request! (input null)");

                var user = await UserManager.FindByEmailAsync(input.Email);

                if (user is null)
                    throw new Exception("Not found a user with given email!");

                var signInResult = await SignInManager.CheckPasswordSignInAsync(user, input.Password, false);

                if (!signInResult.Succeeded)
                    throw new Exception("Password or email not match!");

                var permissionsAndRoles = await RolePermissionRepository.GetUserRolesAndPermissionsByUserIdAsync(user.Id);

                var token = TokenHandlerService.CreateToken(user, permissionsAndRoles.Permissions);

                await UserRefreshTokenRepository.RemoveOldTokensAsync(user.Id);

                await UserRefreshTokenRepository.InsertOneAsync(new UserRefreshToken
                {
                    RefreshToken = token.RefreshToken,
                    AccessToken = token.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Configuration.JWT is not null ? Configuration.JWT.RefreshTokenValidityInDays : 1),
                    UserId = user.Id
                });

                var res = new LoginOutput
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    Roles = permissionsAndRoles.Roles,
                    Permissions = permissionsAndRoles.Permissions
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
                    throw new Exception("Invalid client request! (input null)");

                if ((await UserManager.FindByEmailAsync(input.Email)) is not null)
                    throw new Exception("Email already exist!");

                var user = Activator.CreateInstance<User>();

                user.Email = input.Email;
                user.FirstName = input.FirstName;
                user.LastName = input.LastName;
                user.PhoneNumber = input.PhoneNumber;

                string userName = (input.FirstName + input.LastName).Trim();

                await UserStore.SetUserNameAsync(user, userName, CancellationToken.None);
                await EmailStore.SetEmailAsync(user, input.Email, CancellationToken.None);

                var result = await UserManager.CreateAsync(user, input.Password);

                if (!result.Succeeded)
                {
                    var error = result.Errors.FirstOrDefault();
                    throw new Exception(error is not null ? error.Description : "Create user with usermanager failed!");
                }

                var permissionsAndRoles = await RolePermissionRepository.GetUserRolesAndPermissionsByUserIdAsync(user.Id);
                
                var token = TokenHandlerService.CreateToken(user, permissionsAndRoles.Permissions);

                var refreshTokenExpiryTime = (Configuration.JWT is null || Configuration.JWT.RefreshTokenValidityInDays == 0) ? 1 : Configuration.JWT.RefreshTokenValidityInDays;

                await UserRefreshTokenRepository.InsertOneAsync(new UserRefreshToken
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
                    Roles = permissionsAndRoles.Roles,
                    Permissions = permissionsAndRoles.Permissions
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

                var claims = TokenHandlerService.GetPrincipalFromExpiredToken(input.AccessToken);
                if (claims is null || !claims.Any())
                    throw new Exception("Invalid access token or refresh token");

                var claim = claims.FirstOrDefault(m => m.Type == ClaimTypes.Name);
                if (claim is null)
                    throw new Exception("Invalid access token or refresh token");

                string userId = claim.Value;

                var user = await UserManager.FindByIdAsync(userId);
                if (user is null)
                    throw new Exception("Invalid access token or refresh token");

                var isValid = await UserRefreshTokenRepository.AnyAsync(x => x.UserId == userId && x.RefreshToken == input.RefreshToken && x.RefreshTokenExpiryTime >= DateTime.UtcNow);

                if (!isValid)
                    throw new Exception("Invalid access token or refresh token");

                await UserRefreshTokenRepository.RemoveOldTokensAsync(user.Id);

                var permissionsAndRoles = await RolePermissionRepository.GetUserRolesAndPermissionsByUserIdAsync(user.Id);

                var token = TokenHandlerService.CreateToken(user, permissionsAndRoles.Permissions);

                var refreshTokenExpiryTime = (Configuration.JWT is null || Configuration.JWT.RefreshTokenValidityInDays == 0) ? 1 : Configuration.JWT.RefreshTokenValidityInDays;

                await UserRefreshTokenRepository.InsertOneAsync(new UserRefreshToken
                {
                    RefreshToken = token.RefreshToken,
                    AccessToken = token.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryTime), //TODO: Default Config
                    UserId = user.Id
                });

                return token;
            });
        }
        private IUserEmailStore<User> GetEmailStore()
        {
            if (!UserManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");
            return (IUserEmailStore<User>)UserStore;
        }

        [HttpGet("roles-and-permissions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<GeneralResponse<GetUserRolesAndPermissionsOutput>> GetUserRolesAndPermissionsAsync()
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserId();

                return await RolePermissionRepository.GetUserRolesAndPermissionsByUserIdAsync(userId);
            });
        }
    }
}