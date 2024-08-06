using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;

namespace Framework.AuthServer.Interfaces.Repositories
{
    public interface IUserRefreshTokenRepository : IGenericRepository<UserRefreshToken, int>
    {
        Task UpdateOldTokenAsync(string userId, UserRefreshToken refreshToken);
    }
}
