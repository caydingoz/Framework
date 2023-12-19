using Framework.AuthServer.Enums;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.Shared.Consts;
using Framework.Shared.Enums;
using Framework.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer
{
    internal class DefaultDataMigration
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> UserManager;
        private readonly RoleManager<Role> RoleManager;
        private readonly IPermissionRepository RolePermissionRepository;
        public DefaultDataMigration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            UserManager = serviceProvider.GetService<UserManager<User>>() ?? throw new Exception();
            RoleManager = serviceProvider.GetService<RoleManager<Role>>() ?? throw new Exception();
            RolePermissionRepository = serviceProvider.GetService<IPermissionRepository>() ?? throw new Exception();

        }
        public async Task EnsureMigrationAsync()
        {
            try
            {
                var context = _serviceProvider.GetRequiredService<AuthServerDbContext>();
                await context.Database.MigrateAsync();

                await CreateDefaultRolesAsync();
            
                await CreateDefaultUsersAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task CreateDefaultRolesAsync()
        {
            await CreateRoleAsync(Roles.ADMINISTRATOR_ROLE);
        }

        private async Task CreateDefaultUsersAsync()
        {
            await AddAdministratorAsync("administrator@gmail.com");
        }

        private async Task CreateRoleAsync(string role)
        {
            var roleExists = await RoleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                var newRole = new Role { Id = Guid.NewGuid().ToString(), Name = role, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                var result = await RoleManager.CreateAsync(newRole);
                if (!result.Succeeded)
                    throw new Exception($"{role} role couldn't created!");
            }
        }

        private async Task AddAdministratorAsync(string email)
        {
            User? admin = await UserManager.FindByEmailAsync(email);

            if (admin is null)
            {
                var newAdmin = Activator.CreateInstance<User>();

                newAdmin.UserName = "Administrator";
                newAdmin.Email = email;
                try
                {
                    var result = await UserManager.CreateAsync(newAdmin, "Pass123$"); //TODO: Default config
                    if (!result.Succeeded)
                        throw new Exception("Administrator user couldn't created!");
                }
                catch (Exception ex)
                {
                    throw new Exception("Administrator user couldn't created! Error: " + ex.Message);
                }

                admin = await UserManager.FindByEmailAsync(email);
            }

            if (admin is not null && !await UserManager.IsInRoleAsync(admin, Roles.ADMINISTRATOR_ROLE))
            {
                await UserManager.AddToRoleAsync(admin, Roles.ADMINISTRATOR_ROLE);

                var role = await RoleManager.FindByNameAsync(Roles.ADMINISTRATOR_ROLE) ?? throw new Exception(Roles.ADMINISTRATOR_ROLE + " role not found!");

                PermissionTypes permissionType = PermissionTypes.None;
                permissionType = PermissionHelper.AddPermission(permissionType, PermissionTypes.Read);
                permissionType = PermissionHelper.AddPermission(permissionType, PermissionTypes.Write);
                permissionType = PermissionHelper.AddPermission(permissionType, PermissionTypes.Delete);

                await RolePermissionRepository.InsertOneAsync(new Permission
                {
                    Operation = Pages.Role.ToString(),
                    RoleId = role.Id,
                    Type = permissionType,
                });
            }
        }
    }
}
