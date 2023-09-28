using Framework.Domain.Interfaces.Entities;

namespace Framework.Domain.Entites
{
    public class BaseEntity<T> : IBaseEntity<T>
    {
        public T Id { get; set; }
    }
}
