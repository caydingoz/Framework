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

        public async Task UpdateOldTokenAsync(string userId, UserRefreshToken refreshToken)
        {
            await DbContext.RefreshTokens.Where(x => x.UserId == userId).ExecuteUpdateAsync(x => x
                .SetProperty(y => y.UpdatedAt, DateTime.UtcNow)
                .SetProperty(y => y.AccessToken, refreshToken.AccessToken)
                .SetProperty(y => y.RefreshToken, refreshToken.RefreshToken)
                .SetProperty(y => y.RefreshTokenExpiryTime, refreshToken.RefreshTokenExpiryTime));
        }
    }
}
