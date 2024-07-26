using Framework.Test.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Framework.Test.API
{
    public class TestDbContext : DbContext
    {
        public DbSet<SqlWithManyTestModel> SqlTestModels => Set<SqlWithManyTestModel>();
        public DbSet<SqlWithManyTestRelationModel> SqlWithManyTestRelationModels => Set<SqlWithManyTestRelationModel>();
        public DbSet<SqlWithOneTestModel> SqlWithOneTestModels => Set<SqlWithOneTestModel>();
        public DbSet<SqlWithOneTestRelationModel> SqlWithOneTestRelationModels => Set<SqlWithOneTestRelationModel>();
        public DbSet<CachableTestModel> CachableTestModels => Set<CachableTestModel>();
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SqlWithManyTestModel>().HasMany(x => x.SqlWithManyTestRelationModels).WithMany(x => x.SqlWithManyTestModels).UsingEntity("RelationJoinTable");
            builder.Entity<SqlWithOneTestModel>().HasMany(x => x.SqlWithOneTestRelationModels).WithOne(x => x.SqlWithOneTestModel);
            builder.Entity<CachableTestModel>().OwnsMany(x => x.Childs);
        }
    }
}
