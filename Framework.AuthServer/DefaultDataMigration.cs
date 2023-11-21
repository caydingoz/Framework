using Framework.AuthServer;
using Framework.AuthServer.Models;
using Framework.Shared.Consts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer
{
    internal class DefaultDataMigration
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> UserManager;
        public DefaultDataMigration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            UserManager = serviceProvider.GetService<UserManager<User>>() ?? throw new Exception();

        }
        public async Task EnsureMigrationAsync()
        {
            try
            {
                var context = _serviceProvider.GetRequiredService<AuthServerDbContext>();
                await context.Database.MigrateAsync();

                await CreateDefaultRolesAsync(_serviceProvider);

                await AddAdministratorAsync("administrator@gmail.com");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task CreateDefaultRolesAsync(IServiceProvider provider)
        {
            await CreateRoleAsync(provider, Roles.ADMINISTRATOR_ROLE);
        }

        private static async Task CreateRoleAsync(IServiceProvider provider, string role)
        {
            var manager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            var roleExists = await manager.RoleExistsAsync(role);
            if (!roleExists)
            {
                var newRole = new IdentityRole(role);
                var result = await manager.CreateAsync(newRole);
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

                newAdmin.UserName = Roles.ADMINISTRATOR_ROLE;
                newAdmin.Email = email;

                var result = await UserManager.CreateAsync(newAdmin, "Pass123$"); //TODO: Default config
                if (!result.Succeeded)
                    throw new Exception("Administrator user couldn't created!");

                admin = await UserManager.FindByEmailAsync(email);
            }

            if (admin is not null && !await UserManager.IsInRoleAsync(admin, Roles.ADMINISTRATOR_ROLE))
                await UserManager.AddToRoleAsync(admin, Roles.ADMINISTRATOR_ROLE);
        }
    }
}
