using Framework.Domain.Entites;

namespace Framework.Test.API.Models
{
    public class SqlWithManyTestModel : Entity<int>
    {
        public string Name { get; set; }
        public virtual ICollection<SqlWithManyTestRelationModel> SqlWithManyTestRelationModels { get; set; } = [];
    }

    public class SqlWithManyTestRelationModel : Entity<int>
    {
        public string Name { get; set; }
        public virtual ICollection<SqlWithManyTestModel> SqlWithManyTestModels { get; set; } = [];
    }
}
