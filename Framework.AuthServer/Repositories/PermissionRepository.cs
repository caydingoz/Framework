using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Framework.Shared.Dtos.AuthServer.UserService;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Framework.AuthServer.Repositories
{
    public class PermissionRepository : EfCoreRepositoryBase<Permission, AuthServerDbContext, int>, IPermissionRepository
    {
        public PermissionRepository(AuthServerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<GetUserRolesAndPermissionsOutput> GetUserRolesAndPermissionsByUserIdAsync(string userId)
        {
            var query = from user in DbContext.Users
                        join userRole in DbContext.UserRoles on user.Id equals userRole.UserId
                        join role in DbContext.Roles on userRole.RoleId equals role.Id
                        join permission in DbContext.Permissions on userRole.RoleId equals permission.RoleId into permGroup
                        from perm in permGroup.DefaultIfEmpty()
                        where user.Id == userId
                        select new
                        {
                            role.Name,
                            Operation = perm != null ? perm.Operation : null,
                            PermissionType = perm != null ? perm.Type : 0
                        };

            var data = await query.AsNoTracking().ToListAsync();
            
            return new GetUserRolesAndPermissionsOutput
            {
                Roles = data.DistinctBy(x => x.Name).Select(x => x.Name),
                Permissions = data.Where(x => x.Operation != null).ToDictionary(x => x.Operation, x => x.PermissionType)
            };
        }
    }
}
