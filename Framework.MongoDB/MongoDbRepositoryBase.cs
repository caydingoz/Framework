using Framework.Domain.Extensions;
using Framework.Domain.Interfaces.Entities;
using Framework.Domain.Interfaces.Repositories;
using Framework.Domain.Interfaces.UnitOfWork;
using Framework.Shared.Entities;
using Framework.Shared.Extensions;
using Framework.Shared.Helpers;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.MongoDB
{
    public class MongoDbRepositoryBase<T, U> : IGenericRepositoryWithNonRelation<T, U> where T : class, IBaseEntity<U>, new()
    {
        private IMongoDatabase Database;
        protected IMongoCollection<T> Collection;

        protected MongoDbRepositoryBase(IServiceProvider provider)
        {
            SetDatabaseAndCollection(provider);
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

        private void SetDatabaseAndCollection(IServiceProvider provider)
        {
            try
            {
                var configuration = provider.GetService<MongoDbConfiguration>() ?? throw new Exception("MongoDb credentials missing!");
                var client = new MongoClient(configuration.ConnectionString);
                Database = client.GetDatabase(configuration.Database);
                Collection = Database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
            }
            catch (Exception e)
            {
                throw new Exception("MongoDb Connection Error! Exception : " + e);
            }
        }

        public async Task<T?> GetByIdAsync(U id, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
           => await SingleOrDefaultAsync(x => x.Id.Equals(id), includeLogicalDeleted, cancellationToken);

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.AsQueryable().SingleOrDefaultAsync(cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted, cancellationToken);
            if (cachedData is not null)
                return selector == null ? cachedData.SingleOrDefault() : cachedData.SingleOrDefault(selector);
            return null;
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.AsQueryable().FirstOrDefaultAsync(cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted, cancellationToken);
            if (cachedData is not null)
                return selector == null ? cachedData.FirstOrDefault() : cachedData.FirstOrDefault(selector);
            return null;
        }
        public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> selector, bool includeLogicalDeleted = false, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return Collection.AsQueryable().Where(selector).SortBy(sorts).Paginate(pagination).ToList();

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted, cancellationToken);
            if (cachedData is not null)
                return cachedData.SortBy(sorts).Paginate(pagination);
            return Enumerable.Empty<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync(bool includeLogicalDeleted = false, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return Collection.AsQueryable().SortBy(sorts).Paginate(pagination);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted, cancellationToken);
            if (cachedData is not null)
                return cachedData.SortBy(sorts).Paginate(pagination);
            return Enumerable.Empty<T>();
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.CountDocumentsAsync(selector, cancellationToken: cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted, cancellationToken);
            if (cachedData is not null)
                return selector == null ? cachedData.Count() : cachedData.Count(selector);
            return 0;
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return selector == null ? await Collection.AsQueryable().AnyAsync(cancellationToken) : await Collection.CountDocumentsAsync(selector, cancellationToken: cancellationToken) > 0;

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted, cancellationToken);
            if (cachedData is not null)
                return selector == null ? cachedData.Any() : cachedData.Any(selector);
            return false;
        }
        public async Task<T> InsertOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.Id = EntityExtensions.NewId(entity.Id);
            entity.SetUpdatedAndCreatedDate();

            var options = new InsertOneOptions { BypassDocumentValidation = false };
            await Collection.InsertOneAsync(entity, options, cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheAsync();

            return entity;
        }
        public async Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.Id = EntityExtensions.NewId(entity.Id);
                entity.SetUpdatedAndCreatedDate();
            }

            var options = new InsertManyOptions { BypassDocumentValidation = false };
            await Collection.InsertManyAsync(entities, options, cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheAsync();

            return entities;
        }
        public async Task UpdateOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.SetUpdatedDate();

            await Collection.FindOneAndReplaceAsync(x => x.Id.Equals(entity.Id), entity, cancellationToken: cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheAsync();
        }
        public async Task UpdateManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.SetUpdatedDate();
                await Collection.FindOneAndReplaceAsync(x => x.Id.Equals(entity.Id), entity, cancellationToken: cancellationToken);
            }

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheAsync();
        }
        public async Task DeleteOneAsync(U id, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            var entity = await Collection.FindAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken);

            if (entity is null)
                throw new Exception("Not found an entity with given id!");

            if (IsLogicalDelete)
                await Collection.FindOneAndUpdateAsync(x => x.Id.Equals(id), Builders<T>.Update.Set(x => (x as ILogicalDelete).Deleted, true), cancellationToken: cancellationToken);
            else
                await Collection.DeleteOneAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheAsync();
        }
        public async Task DeleteManyAsync(IEnumerable<U> ids, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            var entities = await Collection.FindAsync(x => ids.Contains(x.Id), cancellationToken: cancellationToken);

            if (IsLogicalDelete)
                await Collection.FindOneAndUpdateAsync(x => ids.Contains(x.Id), Builders<T>.Update.Set(x => (x as ILogicalDelete).Deleted, true), cancellationToken: cancellationToken);
            else
                await Collection.DeleteManyAsync(x => ids.Contains(x.Id), cancellationToken: cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheAsync();
        }
        private async Task<IQueryable<T>?> GetCachedDataAsync(bool includeLogicalDeleted, CancellationToken cancellationToken)
        {
            var json = await CacheDb.StringGetAsync(CacheKey);
            if (json.IsNullOrEmpty)
                return null;
            var data = JsonSerializer.Deserialize<IQueryable<T>>(json);
            if (IsLogicalDelete)
                data = data.WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
            return data;
        }
        private async Task UpdateCacheAsync()
        {
            if (!IsCachable) return;
            await CacheDb.StringSetAsync(CacheKey, JsonSerializer.Serialize(Collection), CacheTimeout);
        }
        private async Task UpdateCacheAsync(object sender, UpdateCacheEventArgs e)
        {
            await UpdateCacheAsync();
        }
    }
    public class UpdateCacheEventArgs : EventArgs
    {
        public CancellationToken CancellationToken { get; set; }
    }
}
