using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos.AuthServer;

namespace Framework.AuthServer.Interfaces.Repositories
{
    public interface IUserPermissionRepository : IGenericRepository<UserPermission, int>
    {
        Task<GetRolesAndPermissionsOutput> GetRolesAndPermissionsByUserIdAsync(string userId);
    }
}
