using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer.Repositories
{
    public class UserTokenRepository : EfCoreRepositoryBase<UserToken, AuthServerDbContext, int>, IUserTokenRepository
    {
        public UserTokenRepository(AuthServerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task UpsertTokenAsync(Guid userId, UserToken refreshToken)
        {
            var tokenCount = await DbContext.UserTokens.CountAsync(x => x.UserId == userId);

            if (tokenCount > 1)
                throw new Exception("Dublicate token data error!");

            if(tokenCount == 0)
                await InsertOneAsync(refreshToken);
            else
                await DbContext.UserTokens.Where(x => x.UserId == userId).ExecuteUpdateAsync(x => x
                    .SetProperty(y => y.UpdatedAt, DateTime.UtcNow)
                    .SetProperty(y => y.AccessToken, refreshToken.AccessToken)
                    .SetProperty(y => y.RefreshToken, refreshToken.RefreshToken)
                    .SetProperty(y => y.RefreshTokenExpiryTime, refreshToken.RefreshTokenExpiryTime));
        }
    }
}
