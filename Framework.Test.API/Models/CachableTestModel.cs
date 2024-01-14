using Framework.Domain.Entites;

namespace Framework.Test.API.Models
{
    public class CachableTestModel : Entity<int>
    {
        public string Name { get; set; }
        public ICollection<CachableTestChildModel> Childs { get; set; }
    }

    public class CachableTestChildModel
    {
        public string Name { get; set; }
    }
}
