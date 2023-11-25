using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos.AuthServer.UserService;

namespace Framework.AuthServer.Interfaces.Repositories
{
    public interface IRolePermissionRepository : IGenericRepository<RolePermission, int>
    {
        Task<GetUserRolesAndPermissionsOutput> GetUserRolesAndPermissionsByUserIdAsync(string userId);
    }
}
