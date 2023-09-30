using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Repositories;
using Framework.EF;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Framework.Test.Repository
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
            Database.OpenConnection();
            Database.EnsureCreated();
        }
        public DbSet<Parent> Parents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Parent>()
                .HasMany(u => u.FirstLayerChilds).WithMany(y => y.Parents);

            builder.Entity<FirstLayerChild>()
                .HasMany(u => u.SecondLayerChilds).WithOne(y => y.FirstLayerChild).HasForeignKey(x => x.FirstLayerChildId);
        }
    }
    public class Parent : BaseEntity<int>
    {
        public ICollection<FirstLayerChild> FirstLayerChilds { get; set; } = new List<FirstLayerChild>();
        public string ParentProp { get; set; } = string.Empty;
    }
    public class FirstLayerChild : BaseEntity<int>
    {
        public ICollection<SecondLayerChild> SecondLayerChilds { get; set; } = new List<SecondLayerChild>();
        public string ChildProp { get; set; } = string.Empty;
        public ICollection<Parent> Parents { get; set; }
    }
    public class SecondLayerChild : BaseEntity<int>
    {
        public string ChildProp { get; set; } = string.Empty;
        public int FirstLayerChildId { get; set; }
        public FirstLayerChild FirstLayerChild { get; set; }
    }

    public class UpdateTestRepository : EfCoreRepositoryBase<Parent, TestDbContext, int>, IUpdateTestRepository
    {
        public UpdateTestRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
    public interface IUpdateTestRepository : IGenericRepository<Parent, int>
    {
    }
}
