﻿using Framework.AuthServer.Models;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer
{
    public class AuthServerDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<UserToken> UserTokens => Set<UserToken>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();
        public AuthServerDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<User>().HasMany(x => x.Roles).WithMany(x => x.Users);
            builder.Entity<Role>().OwnsMany(x => x.Permissions);

            builder.Entity<User>().HasMany(x => x.WorkItems).WithMany(x => x.Users);
            builder.Entity<User>().HasMany(x => x.Activities).WithOne(x => x.User);
            builder.Entity<Activity>().HasOne(x => x.WorkItem).WithMany(x => x.Activities);
        }
    }
}
