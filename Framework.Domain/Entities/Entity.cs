using Framework.Domain.Interfaces.Entities;

namespace Framework.Domain.Entites
{
    public class Entity<T> : BaseEntity<T>, ICreated, IUpdated
    {
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
