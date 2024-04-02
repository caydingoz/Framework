using Framework.Domain.Entites;

namespace Framework.Test.API.Models
{
    public class SqlWithOneTestModel : Entity<int>
    {
        public string Name { get; set; }
        public virtual ICollection<SqlWithOneTestRelationModel> SqlWithOneTestRelationModels { get; set; } = [];
    }

    public class SqlWithOneTestRelationModel : Entity<int>
    {
        public string Name { get; set; }
        public virtual SqlWithOneTestModel SqlWithOneTestModel { get; set; }
    }
}
