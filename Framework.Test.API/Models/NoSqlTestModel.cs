using Framework.Domain.Entites;

namespace Framework.Test.API.Models
{
    public class NoSqlTestModel : Entity<string>
    {
        public string Name { get; set; }
        public ICollection<NoSqlTestChildModel> Childs { get; set; }
    }

    public class NoSqlTestChildModel
    {
        public string Name { get; set; }
    }
}
