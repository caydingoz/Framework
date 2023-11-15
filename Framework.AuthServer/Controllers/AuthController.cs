using Framework.Application;
using Framework.AuthServer.Interfaces.Services;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Framework.Shared.Dtos.AuthServer;
using Framework.Shared.Entities.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Framework.AuthServer.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        Configuration Configuration;
        private readonly ILogger<AuthController> Logger;

        private readonly SignInManager<User> SignInManager;
        private readonly UserManager<User> UserManager;
        private readonly IUserStore<User> UserStore;
        private readonly IUserEmailStore<User> EmailStore;
        private readonly ITokenHandlerService TokenHandlerService;
        private readonly IGenericRepository<UserRefreshToken, int> UserRefreshTokenRepository;

        public AuthController(
            Configuration configuration,
            ILogger<AuthController> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            ITokenHandlerService tokenHandlerService,
            IGenericRepository<UserRefreshToken, int> userRefreshTokenRepository
        )
        {
            Configuration = configuration;
            Logger = logger;
            SignInManager = signInManager;
            UserManager = userManager;
            UserStore = userStore;
            TokenHandlerService = tokenHandlerService;
            UserRefreshTokenRepository = userRefreshTokenRepository;
            EmailStore = GetEmailStore();
        }
        [HttpPost("signin")]
        public async Task<GeneralResponse<TokenOutput>> SignInAsync(EmailSignInInput input)
        {
            return await WithLoggingGeneralResponseAsync(async() =>
            {
                if (input is null)
                    throw new Exception("Invalid client request! (input null)");

                var user = await UserManager.FindByEmailAsync(input.Email);

                if (user is null)
                    throw new Exception("Not found a user with given email");

                var signInResult = await SignInManager.CheckPasswordSignInAsync(user, input.Password, false);

                if (!signInResult.Succeeded)
                    throw new Exception("Password or email not match!");

                var newToken = TokenHandlerService.CreateToken(user);

                var oldTokens = await UserRefreshTokenRepository.WhereAsync(x => x.UserId == new Guid(user.Id));

                if (oldTokens.Count != 0)
                    await UserRefreshTokenRepository.DeleteManyAsync(oldTokens.Select(x => x.Id));

                await UserRefreshTokenRepository.InsertOneAsync(new UserRefreshToken
                {
                    RefreshToken = newToken.RefreshToken,
                    AccessToken = newToken.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Configuration.JWT is not null ? Configuration.JWT.RefreshTokenValidityInDays : 1),
                    UserId = new Guid(user.Id)
                });

                return newToken;
            });
        }

        [HttpPost("signup")]
        public async Task<GeneralResponse<TokenOutput>> SignUpAsync(EmailSignUpInput input)
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

                await SignInManager.SignInAsync(user, isPersistent: false);

                var token = TokenHandlerService.CreateToken(user);

                await UserRefreshTokenRepository.InsertOneAsync(new UserRefreshToken
                {
                    RefreshToken = token.RefreshToken,
                    AccessToken = token.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Configuration.JWT is not null ? Configuration.JWT.RefreshTokenValidityInDays : 1),
                    UserId = new Guid(user.Id)
                });

                return token;
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

                var oldToken = await UserRefreshTokenRepository.SingleOrDefaultAsync(x => x.UserId == new Guid(userId) && x.RefreshToken == input.RefreshToken);

                if (oldToken is null || oldToken.RefreshTokenExpiryTime <= DateTime.UtcNow)
                    throw new Exception("Invalid access token or refresh token");

                var newToken = TokenHandlerService.CreateToken(user);

                await UserRefreshTokenRepository.InsertOneAsync(new UserRefreshToken
                {
                    RefreshToken = newToken.RefreshToken,
                    AccessToken = newToken.AccessToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Configuration.JWT is not null ? Configuration.JWT.RefreshTokenValidityInDays : 1), //TODO: Default Config
                    UserId = new Guid(userId)
                });
                await UserRefreshTokenRepository.DeleteOneAsync(oldToken.Id);

                return newToken;
            });
        }
        private IUserEmailStore<User> GetEmailStore()
        {
            if (!UserManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");
            return (IUserEmailStore<User>)UserStore;
        }
    }
}