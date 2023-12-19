using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer.Repositories
{
    public class UserRefreshTokenRepository : EfCoreRepositoryBase<UserRefreshToken, AuthServerDbContext, int>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(AuthServerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task RemoveOldTokensAsync(string userId)
        {
            await DbContext.RefreshTokens.Where(x => x.UserId == userId).ExecuteDeleteAsync();
        }
    }
}
