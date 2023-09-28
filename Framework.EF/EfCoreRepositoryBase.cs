﻿using Framework.Domain.Extensions;
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
            IsLogicalDelete = typeof(T).GetInterface(typeof(ILogicalDelete).FullName, true) != null;
            IsCachable = typeof(T).GetInterface(typeof(ICachable).FullName, true) != null;
            if (IsCachable)
            {
                CacheKey = (new T() as ICachable).GetCacheKey();
                CacheTimeout = (new T() as ICachable).GetExpireTime();
            }
        }

        private static IDatabase CacheDb => RedisConnectorHelper.Db;
        protected bool IsLogicalDelete { get; }
        protected bool IsCachable { get; }
        protected string CacheKey { get; }
        protected TimeSpan? CacheTimeout { get; }
        protected virtual IQueryable<T> GetDbSetWithAllIncludes(DbContext DbContext) => DbContext.Set<T>();

        public async Task<T?> GetByIdAsync(U id, bool includeAll = false, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
           => await SingleOrDefaultAsync(x => x.Id.Equals(id), includeAll, asNoTracking, includeLogicalDeleted, includes, cancellationToken);

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeAll = false, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeAll, asNoTracking, includeLogicalDeleted, includes);
                return selector == null ? await dbSet.SingleOrDefaultAsync(cancellationToken) : await dbSet.SingleOrDefaultAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(cancellationToken, includeLogicalDeleted);
            if (cachedData is not null)
            {
                return selector == null ? cachedData.SingleOrDefault() : cachedData.SingleOrDefault(selector);
            }
            return null;
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeAll = false, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeAll, asNoTracking, includeLogicalDeleted, includes);
                return selector == null ? await dbSet.FirstOrDefaultAsync(cancellationToken) : await dbSet.FirstOrDefaultAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(cancellationToken, includeLogicalDeleted);
            if (cachedData is not null)
            {
                return selector == null ? cachedData.FirstOrDefault() : cachedData.FirstOrDefault(selector);
            }
            return null;
        }
        public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> selector, bool includeAll = false, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeAll, asNoTracking, includeLogicalDeleted, includes);
                return await dbSet.Where(selector).SortBy(sorts).Paginate(pagination).ToListAsync(cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(cancellationToken, includeLogicalDeleted);
            if (cachedData is not null)
            {
                return cachedData.SortBy(sorts).Paginate(pagination);
            }
            return Enumerable.Empty<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync(bool includeAll = false, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeAll, asNoTracking, includeLogicalDeleted, includes);
                return await dbSet.AsQueryable().SortBy(sorts).Paginate(pagination).ToListAsync(cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(cancellationToken, includeLogicalDeleted);
            if (cachedData is not null)
            {
                return cachedData.SortBy(sorts).Paginate(pagination);
            }
            return Enumerable.Empty<T>();
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeLogicalDeleted);
                return selector == null ? await dbSet.CountAsync(cancellationToken) : await dbSet.CountAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(cancellationToken, includeLogicalDeleted);
            if (cachedData is not null)
            {
                return selector == null ? cachedData.Count() : cachedData.Count(selector);
            }
            return 0;
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
            {
                var dbSet = GetDbSetWithFiltered(includeLogicalDeleted);
                return selector == null ? await dbSet.AnyAsync(cancellationToken) : await dbSet.AnyAsync(selector, cancellationToken);
            }
            var cachedData = await GetCachedDataAsync(cancellationToken, includeLogicalDeleted);
            if (cachedData is not null)
            {
                return selector == null ? cachedData.Any() : cachedData.Any(selector);
            }
            return false;
        }
        public async Task<T> InsertOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            //entity.Id = await new SequentialGuidValueGenerator().NextAsync(DbContext.Attach(entity));
            entity.Id = EntityExtensions.NewId(entity.Id);
            entity.SetUpdatedAndCreatedDate();

            await DbContext.AddAsync(entity, cancellationToken);

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheAsync(cancellationToken);
            }

            return entity;
        }
        public async Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.Id = EntityExtensions.NewId(entity.Id);
                entity.SetUpdatedAndCreatedDate();
            }

            await DbContext.AddRangeAsync(entities, cancellationToken);

            if (unitOfWork is not null)
            {
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheAsync(cancellationToken);
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
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheAsync(cancellationToken);
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
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheAsync(cancellationToken);
            }
        }
        public async Task DeleteOneAsync(U id, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            var entity = await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken);

            if (entity is null)
                throw new Exception("Not found an entity with given id!");

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
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheAsync(cancellationToken);
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
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            }
            else
            {
                await DbContext.SaveChangesAsync(cancellationToken);
                await UpdateCacheAsync(cancellationToken);
            }
        }
        private IQueryable<T> GetDbSetWithFiltered(bool includeLogicalDeleted = false)
        {
            if (IsLogicalDelete)
                return DbContext.Set<T>().WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
            return DbContext.Set<T>();
        }
        private IQueryable<T> GetDbSetWithFiltered(bool includeAll = false, bool asNoTracking = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null)
        {
            var query = includeAll ? GetDbSetWithAllIncludes(DbContext) : DbContext.Set<T>().MultipleInclude(includes);
            if (IsLogicalDelete)
                query = query.WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
            if (asNoTracking)
                query = query.AsNoTracking();
            return query;
        }
        private async Task<IQueryable<T>?> GetCachedDataAsync(CancellationToken cancellationToken, bool includeLogicalDeleted)
        {
            var json = await CacheDb.StringGetAsync(CacheKey);
            if (json.IsNullOrEmpty)
                return null;
            var data = JsonSerializer.Deserialize<IQueryable<T>>(json);
            if (IsLogicalDelete)
                data = data.WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
            return data;
        }
        private async Task UpdateCacheAsync(CancellationToken cancellationToken = default)
        {
            if (!IsCachable) return;
            await CacheDb.StringSetAsync(CacheKey, JsonSerializer.Serialize(await DbContext.Set<T>().ToListAsync(cancellationToken)), CacheTimeout);
        }
        private async Task UpdateCacheAsync(object sender, UpdateCacheEventArgs e)
        {
            await UpdateCacheAsync(e.CancellationToken);
        }
    }
    public class UpdateCacheEventArgs : EventArgs
    {
        public CancellationToken CancellationToken { get; set; }
    }
}
