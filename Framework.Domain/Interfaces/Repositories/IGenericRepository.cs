using Framework.Domain.Interfaces.Entities;

namespace Framework.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T, U> : IGenericQueryRepository<T, U>, IGenericCUDRepository<T, U> where T : IBaseEntity<U>
    {
    }
}
