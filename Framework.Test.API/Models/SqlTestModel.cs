using Framework.Domain.Entites;

namespace Framework.Test.API.Models
{
    public class SqlTestModel : Entity<int>
    {
        public string Name { get; set; }
        public virtual ICollection<SqlTestRelationModel> SqlTestRelationModels { get; set; } = [];
    }

    public class SqlTestRelationModel : Entity<int>
    {
        public string Name { get; set; }
        public virtual ICollection<SqlTestModel> SqlTestModels { get; set; } = [];
    }
}
