namespace Framework.Domain.Interfaces.Entities
{
    public interface IBaseEntity<T>
    {
        public T Id { get; set; }
    }
}
