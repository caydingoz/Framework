using Framework.AuthServer.Enums;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Consts;
using Framework.Shared.Enums;
using Framework.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer
{
    internal class DefaultDataMigration
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGenericRepository<User, Guid> UserRepository;
        private readonly IGenericRepository<Role, int> RoleRepository;
        private readonly IGenericRepository<Permission, int> PermissionRepository;
        public DefaultDataMigration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            UserRepository = serviceProvider.GetService<IGenericRepository<User, Guid>>() ?? throw new Exception();
            RoleRepository = serviceProvider.GetService<IGenericRepository<Role, int>>() ?? throw new Exception();
            PermissionRepository = serviceProvider.GetService<IGenericRepository<Permission, int>>() ?? throw new Exception();

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
            await CreateRoleAsync("Administrator");
        }

        private async Task CreateDefaultUsersAsync()
        {
            await AddAdministratorAsync("administrator@gmail.com");
        }

        private async Task CreateRoleAsync(string roleName)
        {
            var roleExists = await RoleRepository.AnyAsync(x => x.Name == roleName);

            if (!roleExists)
            {
                PermissionTypes permissionType = PermissionTypes.None;
                permissionType = PermissionHelper.AddPermission(permissionType, PermissionTypes.Read);
                permissionType = PermissionHelper.AddPermission(permissionType, PermissionTypes.Write);
                permissionType = PermissionHelper.AddPermission(permissionType, PermissionTypes.Delete);

                var permission = new Permission
                {
                    Operation = Operations.Role,
                    Type = permissionType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var role = new Role 
                { 
                    Name = roleName,
                    Permissions = [permission]
                };

                await RoleRepository.InsertOneAsync(role);
            }
        }

        private async Task AddAdministratorAsync(string email)
        {
            User? admin = await UserRepository.SingleOrDefaultAsync(x => x.Email == email, includes: x => x.Roles.Select(y => y.Permissions));

            if (admin is null)
            {
                var newAdmin = new User
                {
                    FirstName = Roles.ADMINISTRATOR_ROLE,
                    LastName = Roles.ADMINISTRATOR_ROLE,
                    Email = email,
                    Password = "Pass123$"
                };

                admin = await UserRepository.InsertOneAsync(newAdmin);
            }

            if (admin is not null && !admin.Roles.Any(x => x.Name == Roles.ADMINISTRATOR_ROLE))
            {
                var role = await RoleRepository.SingleOrDefaultAsync(x => x.Name == Roles.ADMINISTRATOR_ROLE, includes: x => x.Permissions) 
                        ?? throw new Exception(Roles.ADMINISTRATOR_ROLE + " role not found!");

                admin.Roles.Add(role);

                await UserRepository.UpdateOneAsync(admin);
            }
        }
    }
}
