using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;

namespace Framework.AuthServer.Interfaces.Repositories
{
    public interface IUserTokenRepository : IGenericRepository<UserToken, int>
    {
        Task UpsertTokenAsync(Guid userId, UserToken userToken);
    }
}
