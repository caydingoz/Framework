using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.RoleService.Input;
using Framework.AuthServer.Dtos.RoleService.Output;
using Framework.AuthServer.Enums;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Framework.Shared.Entities;
using Framework.Shared.Entities.Configurations;
using Framework.Shared.Enums;
using Framework.Shared.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class RoleController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<RoleController> Logger;

        private readonly IGenericRepository<Role, int> RoleRepository;
        private readonly IGenericRepository<Permission, int> PermissionRepository;

        public RoleController(
            Configuration configuration,
            ILogger<RoleController> logger,
            IGenericRepository<Role, int> roleRepository,
            IGenericRepository<Permission, int> permissionRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            RoleRepository = roleRepository;
            PermissionRepository = permissionRepository;
        }

        [HttpGet]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetRolesOutput>> GetRolesAsync([FromQuery] int page, [FromQuery] int count, [FromQuery] string? column, [FromQuery] SortTypes? sortType, [FromQuery] string? filterName, [FromQuery] Operations? operation)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var sort = new Sort { Name = column ?? "Id", Type = sortType ?? SortTypes.ASC };
                var pagination = new Pagination { Page = page, Count = count };

                var roles = await RoleRepository.WhereAsync(x => 
                                                    (filterName == null || x.Name.Contains(filterName) || x.Permissions.Any(y => filterName == null || y.Operation.Contains(filterName)))
                                                    && (operation == null || x.Permissions.Any(y => y.Operation.Contains(operation.Value.ToString())))
                                                    , readOnly: true, pagination: pagination, sorts: [sort]);

                var res = new GetRolesOutput();

                foreach (var role in roles)
                    res.Roles.Add(new RolesOutput { Id = role.Id, Name = role.Name ?? string.Empty, CreatedAt = role.CreatedAt, UpdatedAt = role.UpdatedAt });

                res.TotalCount = await RoleRepository.CountAsync(x => filterName == null || x.Name.Contains(filterName));

                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateRoleAsync(CreateRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if (await RoleRepository.AnyAsync(x => x.Name == input.Name))
                    throw new Exception($"There is already exist a role named {input.Name}");

                var role = new Role { Name = input.Name };

                await RoleRepository.InsertOneAsync(role);

                return true;
            });
        }

        [HttpDelete]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteRolesAsync(DeleteRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if (await RoleRepository.AnyAsync(x => input.Ids.Contains(x.Id) && x.Name == "ADMINISTRATOR"))
                    throw new Exception("Changes to the admin role are not allowed!");

                await RoleRepository.DeleteManyAsync(input.Ids);

                return true;
            });
        }

        [HttpGet("permissions")]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.ReadAccess)]
        public GeneralResponse<GetPermissionsOutput> GetAllPermissions()
        {
            return WithLoggingGeneralResponse(() =>
            {
                var res = new GetPermissionsOutput
                {
                    Permissions = Enum.GetValues(typeof(Operations)).Cast<Operations>().Select(v => v.ToString())
                };

                return res;
            });
        }

        [HttpGet("{roleId}/permissions")]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetPermissionsByRoleIdOutput>> GetPermissionsByRoleIdAsync(int roleId, [FromQuery] int page, [FromQuery] int count, [FromQuery] string? column, [FromQuery] SortTypes? sortType)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var sort = new Sort { Name = column ?? "Id", Type = sortType ?? SortTypes.ASC };
                var pagination = new Pagination { Page = page, Count = count };

                var role = await RoleRepository.FirstOrDefaultAsync(x => x.Id == roleId, readOnly: true) ?? throw new Exception("Role not found!");

                var res = new GetPermissionsByRoleIdOutput { RoleId = roleId };

                foreach (var permission in role.Permissions.AsQueryable().SortBy([sort]).Paginate(pagination))
                    res.Permissions.Add(new PermissionsOutput { Id = permission.Id, Operation = permission.Operation.ToString(), Type = permission.Type, CreatedAt = permission.CreatedAt, UpdatedAt = permission.UpdatedAt });

                res.TotalCount = role.Permissions.Count;

                return res;
            });
        }

        [HttpPost("{roleId}/permissions")]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> AddPermissionToRoleAsync([FromRoute] int roleId, AddPermissionToRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var role = await RoleRepository.GetByIdAsync(roleId, includes: x => x.Permissions) ?? throw new Exception("Role is not exist!");

                if (role.Name.Equals(Roles.ADMINISTRATOR_ROLE, StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception("Changes to the admin role are not allowed!");

                var existPermissionNames = role.Permissions.Select(x => x.Operation);

                var newPermissions = input.Permissions.DistinctBy(x => x.Operation).Where(x => !existPermissionNames.Contains(x.Operation.ToString()));

                foreach (var newPermission in newPermissions)
                {
                    role.Permissions.Add(new Permission
                    {
                        Operation = newPermission.Operation.ToString(),
                        Type = newPermission.Type,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                await RoleRepository.UpdateOneAsync(role);

                return true;
            });
        }

        [HttpPut("{roleId}/permissions")]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> UpdatePermissionInRoleAsync([FromRoute] int roleId, UpdatePermissionInRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var role = await RoleRepository.GetByIdAsync(roleId, includes: x => x.Permissions) ?? throw new Exception("Role is not exist!");

                if (role.Name.Equals(Roles.ADMINISTRATOR_ROLE, StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception("Changes to the admin role are not allowed!");

                var notExistPermissionIds = input.Permissions.DistinctBy(x => x.Id).Where(x => !role.Permissions.Any(y => y.Id == x.Id));

                if (notExistPermissionIds.Any())
                    throw new Exception("Some permissionIds not found in role! Ids: " + string.Join(", ", notExistPermissionIds.Select(x => x.Id)));

                foreach (var permissionToUpdate in input.Permissions.DistinctBy(x => x.Id))
                {
                    var permission = role.Permissions.Single(x => x.Id == permissionToUpdate.Id);
                    permission.Type = permissionToUpdate.Type;
                    permission.UpdatedAt = DateTime.UtcNow;
                }

                await RoleRepository.UpdateOneAsync(role);

                return true;
            });
        }

        [HttpDelete("{roleId}/permissions")]
        [Authorize(Policy = OperationNames.Role + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> RemovePermissionFromRoleAsync([FromRoute] int roleId, RemovePermissionFromRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var role = await RoleRepository.GetByIdAsync(roleId, includes: x => x.Permissions) ?? throw new Exception("Role is not exist!");

                if (role.Name.Equals(Roles.ADMINISTRATOR_ROLE, StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception("Changes to the admin role are not allowed!");

                var notExistPermissionIds = input.PermissionIds.Distinct().Where(x => !role.Permissions.Any(y => y.Id == x));

                if (notExistPermissionIds.Any())
                    throw new Exception("Some permissionIds not found in role! Ids: " + string.Join(", ", notExistPermissionIds));

                var permissionsToRemove = role.Permissions.Where(x => input.PermissionIds.Contains(x.Id)).ToList();

                foreach (var permissionToRemove in permissionsToRemove)
                {
                    role.Permissions.Remove(permissionToRemove);
                }

                await RoleRepository.UpdateOneAsync(role);

                return true;
            });
        }
    }
}