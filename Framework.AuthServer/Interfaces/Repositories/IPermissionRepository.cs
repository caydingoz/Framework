using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos.AuthServer.UserService;

namespace Framework.AuthServer.Interfaces.Repositories
{
    public interface IPermissionRepository : IGenericRepository<Permission, int>
    {
        Task<GetUserRolesAndPermissionsOutput> GetUserRolesAndPermissionsByUserIdAsync(string userId);
    }
}
