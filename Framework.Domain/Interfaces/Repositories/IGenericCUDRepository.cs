using Framework.Domain.Interfaces.UnitOfWork;
using Framework.Domain.Interfaces.Entities;

namespace Framework.Domain.Interfaces.Repositories
{
    public interface IGenericCUDRepository<T, U> where T : IBaseEntity<U>
    {
        Task<T> InsertOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default);
        Task UpdateOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default);
        Task UpdateManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default);
        Task DeleteOneAsync(U id, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default);
        Task DeleteManyAsync(IEnumerable<U> ids, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default);
    }
}
