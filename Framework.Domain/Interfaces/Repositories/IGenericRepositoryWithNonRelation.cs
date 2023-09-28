using Framework.Domain.Interfaces.Entities;

namespace Framework.Domain.Interfaces.Repositories
{
    public interface IGenericRepositoryWithNonRelation<T, U> : IGenericQueryRepositoryWithNonRelation<T, U>, IGenericCUDRepository<T, U> where T : IBaseEntity<U>
    {
    }
}
