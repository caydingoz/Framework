using Framework.AuthServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer
{
    public class AuthServerDbContext : IdentityDbContext<User, Role, string>
    {
        public DbSet<UserRefreshToken> RefreshTokens => Set<UserRefreshToken>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public AuthServerDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Role>().HasMany(x => x.Permissions).WithOne(x => x.Role);
        }
    }
}
