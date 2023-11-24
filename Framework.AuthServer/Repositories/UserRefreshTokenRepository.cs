using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Framework.Shared.Dtos.AuthServer;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Framework.AuthServer.Repositories
{
    public class UserRefreshTokenRepository : EfCoreRepositoryBase<UserRefreshToken, AuthServerDbContext, int>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(AuthServerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task RemoveOldTokensAsync(string userId)
        {
            await DbContext.UserRefreshTokens.Where(x => x.UserId == userId).ExecuteDeleteAsync();
        }
    }
}
