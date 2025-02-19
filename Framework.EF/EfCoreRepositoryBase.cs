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
    public class EfCoreRepositoryBase<T, TDbContext, U> : IGenericRepository<T, U> where T : class, IBaseEntity<U> where TDbContext : DbContext
    {
        private static IDatabase CacheDb => RedisConnectorHelper.Db;
        private bool IsLogicalDelete { get; }
        private bool IsCachable { get; }
        protected TDbContext DbContext { get; }
        public EfCoreRepositoryBase(TDbContext dbContext)
        {
            DbContext = dbContext;
            IsLogicalDelete = InterfaceExistenceChecker.Check<T>(typeof(ILogicalDelete));
            IsCachable = InterfaceExistenceChecker.Check<T>(typeof(ICachable));
        }

        public async Task<T?> GetByIdAsync(U id, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (id is null)
                throw new Exception("Id is null!");

            if (IsCachable && readOnly)
                return await GetCacheByIdAsync(id, includeLogicalDeleted);

            return await SingleOrDefaultAsync(x => x.Id.Equals(id), readOnly, includeLogicalDeleted, includes, cancellationToken);
        }

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? filter = null, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (IsCachable && readOnly)
            {
                var cachedData = await GetCacheAsync(includeLogicalDeleted);
                if (cachedData is null)
                    return null;
                return filter == null ? cachedData.SingleOrDefault() : cachedData.SingleOrDefault(filter);
            }
            var dbSet = GetDbSetWithFiltered(readOnly, includeLogicalDeleted, includes);
            return filter == null ? await dbSet.SingleOrDefaultAsync(cancellationToken) : await dbSet.SingleOrDefaultAsync(filter, cancellationToken);
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? filter = null, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (IsCachable && readOnly)
            {
                var cachedData = await GetCacheAsync(includeLogicalDeleted);
                if (cachedData is null)
                    return null;
                return filter == null ? cachedData.FirstOrDefault() : cachedData.FirstOrDefault(filter);
            }
            var dbSet = GetDbSetWithFiltered(readOnly, includeLogicalDeleted, includes);
            return filter == null ? await dbSet.FirstOrDefaultAsync(cancellationToken) : await dbSet.FirstOrDefaultAsync(filter, cancellationToken);
        }
        public async Task<ICollection<T>> WhereAsync(Expression<Func<T, bool>> filter, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (IsCachable && readOnly)
            {
                var cachedData = await GetCacheAsync(includeLogicalDeleted);
                if (cachedData is null)
                    return Enumerable.Empty<T>().ToList();
                return cachedData.Where(filter).SortBy(sorts).Paginate(pagination).ToList();
            }
            var dbSet = GetDbSetWithFiltered(readOnly, includeLogicalDeleted, includes);
            return await dbSet.Where(filter).SortBy(sorts).Paginate(pagination).ToListAsync(cancellationToken);
        }
        public async Task<ICollection<TResult>> WhereWithSelectAsync<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (IsCachable && readOnly)
            {
                var cachedData = await GetCacheAsync(includeLogicalDeleted);
                if (cachedData is null)
                    return Enumerable.Empty<TResult>().ToList();
                return cachedData.SortBy(sorts).Paginate(pagination).Select(selector).ToList();
            }
            var dbSet = GetDbSetWithFiltered(readOnly, includeLogicalDeleted, includes);
            return await dbSet.Where(filter).Select(selector).SortBy(sorts).Paginate(pagination).ToListAsync(cancellationToken);
        }
        public async Task<ICollection<T>> GetAllAsync(bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (IsCachable && readOnly)
            {
                var cachedData = await GetCacheAsync(includeLogicalDeleted);
                if (cachedData is null)
                    return Enumerable.Empty<T>().ToList();
                return cachedData.SortBy(sorts).Paginate(pagination).ToList();
            }
            var dbSet = GetDbSetWithFiltered(readOnly, includeLogicalDeleted, includes);
            return await dbSet.SortBy(sorts).Paginate(pagination).ToListAsync(cancellationToken);
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>>? filter = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeLogicalDeleted);
                return filter == null ? await dbSet.CountAsync(cancellationToken) : await dbSet.CountAsync(filter, cancellationToken);
            }
            var cachedData = await GetCacheAsync(includeLogicalDeleted);
            if (cachedData is null)
                return 0;
            return filter == null ? cachedData.Count() : cachedData.Count(filter);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeLogicalDeleted);
                return filter == null ? await dbSet.AnyAsync(cancellationToken) : await dbSet.AnyAsync(filter, cancellationToken);
            }
            var cachedData = await GetCacheAsync(includeLogicalDeleted);
            if (cachedData is null)
                return false;
            return filter == null ? cachedData.Any() : cachedData.Any(filter);
        }
        public async Task<T> InsertOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.SetUpdatedAndCreatedDate();
            var entryEntity = await DbContext.AddAsync(entity, cancellationToken);

            if (typeof(U) == typeof(Guid) && (entity.Id == null || entity.Id.ToString() == Guid.Empty.ToString()))
                entity.Id = (U)Convert.ChangeType(await new SequentialGuidValueGenerator().NextAsync(entryEntity, cancellationToken), typeof(U));

            if (unitOfWork is not null)
            {
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await UpsertCacheAsync(s, new UpsertCacheEventArgs<T>
                    {
                        Entities = [entity]
                    });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                if (IsCachable)
                    await UpsertCacheAsync([entity]);
            }

            return entity;
        }
        public async Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                //if (typeof(U) == typeof(Guid) && (entity.Id == null || entity.Id.ToString() == Guid.Empty.ToString()))
                //    entity.Id = (U)Convert.ChangeType(await new SequentialGuidValueGenerator().NextAsync(DbContext.Attach(entity), cancellationToken), typeof(U));
                entity.SetUpdatedAndCreatedDate();
            }

            await DbContext.AddRangeAsync(entities, cancellationToken);

            if (unitOfWork is not null)
            {
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await UpsertCacheAsync(s, new UpsertCacheEventArgs<T>
                    {
                        Entities = entities
                    });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                if (IsCachable)
                    await UpsertCacheAsync(entities);
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
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await UpsertCacheAsync(s, new UpsertCacheEventArgs<T>
                    {
                        Entities = [entity]
                    });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                if (IsCachable)
                    await UpsertCacheAsync([entity]);
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
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await UpsertCacheAsync(s, new UpsertCacheEventArgs<T>
                    {
                        Entities = entities
                    });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                if (IsCachable)
                    await UpsertCacheAsync(entities);
            }
        }
        public async Task DeleteOneAsync(U id, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            if (id is null)
                throw new Exception("Id is null!");
            var entity = await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken) ?? throw new Exception("Not found a data with given id!");
            
            if (IsLogicalDelete)
            {
                (entity as ILogicalDelete).IsDeleted = true;
                DbContext.Set<T>().Update(entity);
            }
            else
            {
                DbContext.Set<T>().Remove(entity);
            }

            if (unitOfWork is not null)
            {
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await DeleteCacheAsync(s, new DeleteCacheEventArgs<U>
                    {
                        Ids = [id]
                    });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                if (IsCachable)
                    await DeleteCacheAsync([id]);
            }
        }
        public async Task DeleteManyAsync(IEnumerable<U> ids, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            var entities = DbContext.Set<T>().Where(x => ids.Contains(x.Id));

            if (IsLogicalDelete)
            {
                (entities as IEnumerable<ILogicalDelete>).ToList().ForEach(x => x.IsDeleted = true);
                DbContext.Set<T>().UpdateRange(entities);
            }
            else
            {
                DbContext.Set<T>().RemoveRange(entities);
            }

            if (unitOfWork is not null)
            {
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await DeleteCacheAsync(s, new DeleteCacheEventArgs<U>
                    {
                        Ids = ids.ToArray()
                    });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                if (IsCachable)
                    await DeleteCacheAsync(ids.ToArray());
            }
        }
        private IQueryable<T> GetDbSetWithFiltered(bool includeLogicalDeleted = false)
        {
            if (IsLogicalDelete)
                return DbContext.Set<T>().WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.IsDeleted)}={includeLogicalDeleted.ToString().ToLower()}");
            return DbContext.Set<T>();
        }
        private IQueryable<T> GetDbSetWithFiltered(bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null)
        {
            var query = DbContext.Set<T>().MultipleInclude(includes);
            if (IsLogicalDelete && includeLogicalDeleted)
                query = query.IgnoreQueryFilters();
            if (asNoTracking)
                query = query.AsNoTracking();
            return query;
        }
        private async Task<T?> GetCacheByIdAsync(U Id, bool includeLogicalDeleted)
        {
            RedisValue value = await CacheDb.StringGetAsync($"{typeof(T).FullName}:{Id}");
            if (value.IsNullOrEmpty)
                return null;
#pragma warning disable CS8604 // Possible null reference argument.
            var data = JsonSerializer.Deserialize<T>(value);
#pragma warning restore CS8604 // Possible null reference argument.
            if (IsLogicalDelete && !includeLogicalDeleted && (data as ILogicalDelete).IsDeleted)
                return null;
            return data;
        }
        private async Task<IQueryable<T>?> GetCacheAsync(bool includeLogicalDeleted)
        {
            RedisKey[] matchingKeys = CacheDb.Multiplexer.GetServer(CacheDb.Multiplexer.GetEndPoints().First())
                .Keys(pattern: $"{typeof(T).FullName}:*").ToArray();

            RedisValue[] values = await CacheDb.StringGetAsync(matchingKeys);
            List<T> dataList = [];

            foreach (var value in values)
            {
                if (!value.IsNull)
                {
                    string jsonString = value.ToString();
#pragma warning disable CS8600 // Possible null reference argument.
                    T deserializedObject = JsonSerializer.Deserialize<T>(jsonString);
#pragma warning restore CS8600 // Possible null reference argument.
                    if(deserializedObject != null)
                        dataList.Add(deserializedObject);
                }
            }
            if (IsLogicalDelete)
                return dataList.AsQueryable().WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.IsDeleted)}={includeLogicalDeleted.ToString().ToLower()}");
            return dataList.AsQueryable();
        }
        private static async Task UpsertCacheAsync(IEnumerable<T> entities)
        {
            var keyValuePairs = new List<KeyValuePair<RedisKey, RedisValue>>();

            foreach (var entity in entities)
                keyValuePairs.Add(new KeyValuePair<RedisKey, RedisValue>($"{typeof(T).FullName}:{entity.Id}", JsonSerializer.Serialize(entity)));

            await CacheDb.StringSetAsync(keyValuePairs.ToArray());
        }
        private static async Task DeleteCacheAsync(U[] ids)
        {
            var keys = new List<RedisKey>();
            foreach (var id in ids)
                keys.Add($"{typeof(T).FullName}:{id}");

            await CacheDb.KeyDeleteAsync(keys.ToArray());
        }
        private static async Task UpsertCacheAsync(object sender, UpsertCacheEventArgs<T> e) => await UpsertCacheAsync(e.Entities);
        private static async Task DeleteCacheAsync(object sender, DeleteCacheEventArgs<U> e) => await DeleteCacheAsync(e.Ids);
    }
    public class UpsertCacheEventArgs<T> : EventArgs
    {
        public required IEnumerable<T> Entities { get; set; }
    }
    public class DeleteCacheEventArgs<U> : EventArgs
    {
        public required U[] Ids { get; set; }
    }
}
