using Framework.Test.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Framework.Test.API
{
    public class TestDbContext : DbContext
    {
        public DbSet<SqlTestModel> SqlTestModels => Set<SqlTestModel>();
        public DbSet<SqlTestRelationModel> SqlTestRelationModels => Set<SqlTestRelationModel>();
        public DbSet<CachableTestModel> CachableTestModels => Set<CachableTestModel>();
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SqlTestModel>().HasMany(x => x.SqlTestRelationModels).WithMany(x => x.SqlTestModels);//.UsingEntity("RelationJoinTable");
            builder.Entity<CachableTestModel>().OwnsMany(x => x.Childs);
        }
    }
}
