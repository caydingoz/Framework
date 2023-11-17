using Framework.Domain.Extensions;
using Framework.Domain.Interfaces.Entities;
using Framework.Domain.Interfaces.Repositories;
using Framework.Domain.Interfaces.UnitOfWork;
using Framework.EF.Extensions;
using Framework.Shared.Entities;
using Framework.Shared.Extensions;
using Framework.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using StackExchange.Redis;
using System.Linq.Expressions;
using System.Text.Json;

namespace Framework.EF
{
    public class EfCoreRepositoryBase<T, TDbContext, U> : IGenericRepository<T, U> where T : class, IBaseEntity<U>, new() where TDbContext : DbContext
    {
        protected TDbContext DbContext { get; }
        public EfCoreRepositoryBase(TDbContext dbContext)
        {
            DbContext = dbContext;
            IsLogicalDelete = InterfaceExistenceChecker.Check<T>(typeof(ILogicalDelete));
            IsCachable = InterfaceExistenceChecker.Check<T>(typeof(ICachable));
            if (IsCachable)
                CacheKey = $"{typeof(T).GetType().FullName}";
        }

        private static IDatabase CacheDb => RedisConnectorHelper.Db;
        protected bool IsLogicalDelete { get; }
        protected bool IsCachable { get; }
        protected string? CacheKey { get; }

        public async Task<T?> GetByIdAsync(U id, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (id is null)
                throw new Exception("Id is null!");
            return await SingleOrDefaultAsync(x => x.Id.Equals(id), asNoTracking, includeLogicalDeleted, includes, cancellationToken);
        }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(asNoTracking, includeLogicalDeleted, includes);
                return selector == null ? await dbSet.SingleOrDefaultAsync(cancellationToken) : await dbSet.SingleOrDefaultAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return null;
            return selector == null ? cachedData.SingleOrDefault() : cachedData.SingleOrDefault(selector);
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(asNoTracking, includeLogicalDeleted, includes);
                return selector == null ? await dbSet.FirstOrDefaultAsync(cancellationToken) : await dbSet.FirstOrDefaultAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return null;
            return selector == null ? cachedData.FirstOrDefault() : cachedData.FirstOrDefault(selector);
        }
        public async Task<ICollection<T>> WhereAsync(Expression<Func<T, bool>> selector, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(asNoTracking, includeLogicalDeleted, includes);
                return await dbSet.Where(selector).SortBy(sorts).Paginate(pagination).ToListAsync(cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return Enumerable.Empty<T>().ToList();
            return cachedData.SortBy(sorts).Paginate(pagination).ToList();
        }
        public async Task<ICollection<T>> GetAllAsync(bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(asNoTracking, includeLogicalDeleted, includes);
                return await dbSet.SortBy(sorts).Paginate(pagination).ToListAsync(cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return Enumerable.Empty<T>().ToList();
            return cachedData.SortBy(sorts).Paginate(pagination).ToList();
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeLogicalDeleted);
                return selector == null ? await dbSet.CountAsync(cancellationToken) : await dbSet.CountAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return 0;
            return selector == null ? cachedData.Count() : cachedData.Count(selector);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeLogicalDeleted);
                return selector == null ? await dbSet.AnyAsync(cancellationToken) : await dbSet.AnyAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return false;
            return selector == null ? cachedData.Any() : cachedData.Any(selector);
        }
        public async Task<T> InsertOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.SetUpdatedAndCreatedDate();
            var entryEntity = await DbContext.AddAsync(entity, cancellationToken);

            if (typeof(U) == typeof(Guid) && (entity.Id == null || entity.Id.ToString() == Guid.Empty.ToString()))
                entity.Id = (U)Convert.ChangeType(await new SequentialGuidValueGenerator().NextAsync(entryEntity, cancellationToken), typeof(U));

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheIfCachableAsync(cancellationToken);
            }

            return entity;
        }
        public async Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                if (typeof(U) == typeof(Guid) && (entity.Id == null || entity.Id.ToString() == Guid.Empty.ToString()))
                    entity.Id = (U)Convert.ChangeType(await new SequentialGuidValueGenerator().NextAsync(DbContext.Attach(entity), cancellationToken), typeof(U));
                entity.SetUpdatedAndCreatedDate();
            }

            await DbContext.AddRangeAsync(entities, cancellationToken);

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheIfCachableAsync(cancellationToken);
            }

            return entities;
        }
        public async Task UpdateOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.SetUpdatedDate();

            //DbContext harici(cache, dto mapping to T) gelen veri sorunları:
            //entity detached statusunde geliyor.
            //alt listeyi ownsmany olarakta tanımlasan dışarıdan geldiği için ilgili alanı updated'a çekmen gerek
            //aşağıdaki method sadece T classına etki ediyor.
            if (DbContext.Entry(entity).State == EntityState.Detached)
                DbContext.Update(entity);

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheIfCachableAsync(cancellationToken);
            }
        }
        public async Task UpdateManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.SetUpdatedDate();

                if (DbContext.Entry(entity).State == EntityState.Detached)
                    DbContext.Update(entity);
            }

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheIfCachableAsync(cancellationToken);
            }
        }
        public async Task DeleteOneAsync(U id, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            if (id is null)
                throw new Exception("Id is null!");
            var entity = await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken) ?? throw new Exception("Not found an entity with given id!");
            
            if (IsLogicalDelete)
            {
                (entity as ILogicalDelete).Deleted = true;
                DbContext.Set<T>().Update(entity);
            }
            else
            {
                DbContext.Set<T>().Remove(entity);
            }

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheIfCachableAsync(cancellationToken);
            }
        }
        public async Task DeleteManyAsync(IEnumerable<U> ids, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            var entities = DbContext.Set<T>().Where(x => ids.Contains(x.Id));

            if (IsLogicalDelete)
            {
                (entities as IEnumerable<ILogicalDelete>).ToList().ForEach(x => x.Deleted = true);
                DbContext.Set<T>().UpdateRange(entities);
            }
            else
            {
                DbContext.Set<T>().RemoveRange(entities);
            }

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheIfCachableAsync(cancellationToken);
            }
        }
        private IQueryable<T> GetDbSetWithFiltered(bool includeLogicalDeleted = false)
        {
            if (IsLogicalDelete)
                return DbContext.Set<T>().WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
            return DbContext.Set<T>();
        }
        private IQueryable<T> GetDbSetWithFiltered(bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null)
        {
            var query = DbContext.Set<T>().MultipleInclude(includes);
            if (IsLogicalDelete)
                query = query.WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
            if (asNoTracking)
                query = query.AsNoTracking();
            return query;
        }
        private async Task<IQueryable<T>?> GetCachedDataAsync(bool includeLogicalDeleted)
        {
            var json = await CacheDb.StringGetAsync(CacheKey);
            if (json.IsNullOrEmpty)
                return null;
#pragma warning disable CS8604 // Possible null reference argument.
            var data = JsonSerializer.Deserialize<IEnumerable<T>>(json);
#pragma warning restore CS8604 // Possible null reference argument.
            if (data is null)
                return null;
            if (IsLogicalDelete)
                data = data.AsQueryable().WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
            return data.AsQueryable();
        }
        private async Task UpdateCacheIfCachableAsync(CancellationToken cancellationToken = default) //TODO: fix with insert, update methods
        {
            if (!IsCachable) return;
            await CacheDb.StringSetAsync(CacheKey, JsonSerializer.Serialize(await DbContext.Set<T>().ToListAsync(cancellationToken)));
        }
        private async Task UpdateCacheIfCachableAsync(object sender, UpdateCacheEventArgs e) => await UpdateCacheIfCachableAsync(e.CancellationToken);
    }
    public class UpdateCacheEventArgs : EventArgs
    {
        public CancellationToken CancellationToken { get; set; }
    }
}
