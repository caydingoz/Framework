using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Framework.Shared.Dtos.AuthServer.RoleService;
using Framework.Shared.Entities;
using Framework.Shared.Entities.Configurations;
using Framework.Shared.Enums;
using Framework.Shared.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class RolesController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<RolesController> Logger;

        private readonly RoleManager<Role> RoleManager;
        private readonly IPermissionRepository PermissionRepository;

        public RolesController(
            Configuration configuration,
            ILogger<RolesController> logger,
            IPermissionRepository permissionRepository,
            RoleManager<Role> roleManager
            )
        {
            Configuration = configuration;
            Logger = logger;
            PermissionRepository = permissionRepository;
            RoleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetRolesOutput>> GetRolesAsync([FromQuery] int page, [FromQuery] int count, [FromQuery] string? column, [FromQuery] SortTypes sortType)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var sort = new Sort { Name = column ?? "Id", Type = sortType };
                var roles = await RoleManager.Roles.SortBy(new[] { sort }).Skip(page * count).Take(count).ToListAsync();
                var res = new GetRolesOutput();

                foreach (var role in roles)
                    res.Roles.Add(new RolesOutput { Id = role.Id, Name = role.Name ?? string.Empty, CreatedAt = role.CreatedAt, UpdatedAt = role.UpdatedAt });

                res.TotalCount = await RoleManager.Roles.CountAsync();
                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> AddRoleAsync(AddRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if (await RoleManager.Roles.AnyAsync(x => x.Name == input.Name))
                    throw new Exception($"There is already exist a role named {input.Name}");

                await RoleManager.CreateAsync(new Role { Id = Guid.NewGuid().ToString(), Name = input.Name, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });

                return true;
            });
        }

        [HttpDelete]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteRolesAsync(DeleteRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if (await RoleManager.Roles.AnyAsync(x => input.Ids.Contains(x.Id) && x.NormalizedName == "ADMINISTRATOR"))
                    throw new Exception("Changes to the admin role are not allowed!");

                await RoleManager.Roles.Where(x => input.Ids.Contains(x.Id)).ExecuteDeleteAsync();

                return true;
            });
        }

        [HttpGet("{roleId}/permissions")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetPermissionsByRoleIdOutput>> GetPermissionsByRoleIdAsync(string roleId, [FromQuery] int page, [FromQuery] int count, [FromQuery] string? column, [FromQuery] SortTypes sortType)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var sort = new Sort { Name = column ?? "Id", Type = sortType };
                var permissions = await PermissionRepository.WhereAsync(x => x.RoleId == roleId, readOnly: true, pagination: new Pagination { Page = page, Count = count }, sorts: new[] { sort });

                var res = new GetPermissionsByRoleIdOutput { RoleId = roleId };

                foreach (var permission in permissions)
                    res.Permissions.Add(new PermissionsOutput { Id = permission.Id, Operation = permission.Operation, Type = permission.Type, CreatedAt = permission.CreatedAt, UpdatedAt = permission.UpdatedAt });

                res.TotalCount = await PermissionRepository.CountAsync(x => x.RoleId == roleId);
                return res;
            });
        }

        [HttpPost("{roleId}/permissions")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> AddPermissionAsync([FromRoute] string roleId, AddPermissionByRoleIdInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var role = await RoleManager.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == roleId) ?? throw new Exception("Role is not exist!");

                if (role.NormalizedName == "ADMINISTRATOR")
                    throw new Exception("Changes to the admin role are not allowed!");

                if (role.Permissions is not null && role.Permissions.Any(x => x.RoleId == roleId && x.Operation == input.Operation))
                    throw new Exception("Permission is exist!");

                var permission = new Permission { RoleId = roleId, Operation = input.Operation, Type = input.PermissionType };

                role.Permissions ??= new List<Permission>();

                role.Permissions.Add(permission);

                await RoleManager.UpdateAsync(role);

                return true;
            });
        }

        [HttpPut("{roleId}/permissions")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> UpdatePermissionAsync([FromRoute] string roleId, UpdatePermissionByRoleIdInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var role = await RoleManager.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == roleId) ?? throw new Exception("Role is not exist!");

                if (role.NormalizedName == "ADMINISTRATOR")
                    throw new Exception("Changes to the admin role are not allowed!");

                if (role.Permissions is null || role.Permissions.Count == 0)
                    throw new Exception("Permission is not exist!");

                var permission = role.Permissions.FirstOrDefault(x => x.Id == input.Id) ?? throw new Exception("Permission is not exist with given Id!");

                permission.Type = input.PermissionType;

                await RoleManager.UpdateAsync(role);

                return true;
            });
        }

        [HttpDelete("{roleId}/permissions")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeletePermissionAsync([FromRoute] string roleId, DeletePermissionByRoleIdInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var role = await RoleManager.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == roleId) ?? throw new Exception("Role is not exist!");

                if (role.NormalizedName == "ADMINISTRATOR")
                    throw new Exception("Changes to the admin role are not allowed!");

                if (role.Permissions is null || role.Permissions.Count == 0)
                    throw new Exception("Permission is not exist!");

                var permission = role.Permissions.FirstOrDefault(x => x.Id == input.Id) ?? throw new Exception("Permission is not exist with given Id!");

                role.Permissions.Remove(permission);

                await RoleManager.UpdateAsync(role);

                return true;
            });
        }
    }
}