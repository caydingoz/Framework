using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Framework.Shared.Dtos.AuthServer.RoleService;
using Framework.Shared.Entities;
using Framework.Shared.Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class RoleController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<RoleController> Logger;

        private readonly RoleManager<IdentityRole> RoleManager;
        private readonly IRolePermissionRepository RolePermissionRepository;

        public RoleController(
            Configuration configuration,
            ILogger<RoleController> logger,
            IRolePermissionRepository rolePermissionRepository,
            RoleManager<IdentityRole> roleManager
            )
        {
            Configuration = configuration;
            Logger = logger;
            RolePermissionRepository = rolePermissionRepository;
            RoleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetRolesOutput>> GetRolesAsync([FromQuery] int page, [FromQuery] int count)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var roles = RoleManager.Roles.Skip(page * count).Take(count);
                var res = new GetRolesOutput();

                foreach (var role in roles)
                    res.Roles.Add(new RolesOutput { Id = role.Id, Name = role.Name ?? string.Empty });

                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> AddRoleAsync(AddRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                await RoleManager.CreateAsync(new IdentityRole { Name = input.Name });

                return true;
            });
        }

        [HttpDelete]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteRoleAsync(DeleteRoleInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var role = RoleManager.Roles.SingleOrDefault(x => x.Id == input.Id) ?? throw new Exception("Role is not exist!");

                await RoleManager.DeleteAsync(role);

                return true;
            });
        }

        [HttpGet("{roleId}/permission")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetPermissionsByRoleIdOutput>> GetPermissionsByRoleIdAsync(string roleId, [FromQuery] int page, [FromQuery] int count)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var permissions = await RolePermissionRepository.WhereAsync(x => x.RoleId == roleId, asNoTracking: true, pagination: new Pagination { Page = page, Count = count });

                var res = new GetPermissionsByRoleIdOutput { RoleId = roleId };

                foreach (var permission in permissions)
                    res.Permissions.Add(new PermissionsOutput { Id = permission.Id, Operation = permission.Operation, PermissionType = permission.PermissionType });

                return res;
            });
        }

        [HttpPost("{roleId}/permission")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> AddPermissionAsync([FromRoute]string roleId, AddPermissionByRoleIdInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if (await RolePermissionRepository.AnyAsync(x => x.RoleId == roleId && x.Operation == input.Operation))
                    throw new Exception("Permission is exist!");

                var permission = new RolePermission { RoleId = roleId, Operation = input.Operation, PermissionType = input.PermissionType };

                await RolePermissionRepository.InsertOneAsync(permission);

                return true;
            });
        }

        [HttpPut("{roleId}/permission")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> UpdatePermissionAsync(UpdatePermissionByRoleIdInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var permission = await RolePermissionRepository.GetByIdAsync(input.Id) ?? throw new Exception("Permission is not exist!");

                permission.PermissionType = input.PermissionType;

                await RolePermissionRepository.UpdateOneAsync(permission);

                return true;
            });
        }

        [HttpDelete("{roleId}/permission")]
        [Authorize(Policy = PageNames.Role + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeletePermissionAsync(DeletePermissionByRoleIdInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                await RolePermissionRepository.DeleteOneAsync(input.Id);

                return true;
            });
        }
    }
}