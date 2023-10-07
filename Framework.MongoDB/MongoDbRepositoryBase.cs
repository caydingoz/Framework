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
using MongoDB.Bson;
using Framework.MongoDB.Extensions;

namespace Framework.MongoDB
{
    public class MongoDbRepositoryBase<T, U> : IGenericRepositoryWithNonRelation<T, U> where T : class, IBaseEntity<U>, new()
    {
        private IMongoDatabase? Database;
        protected IMongoCollection<T> Collection;

        public MongoDbRepositoryBase(IServiceProvider provider)
        {
            SetDatabaseAndCollection(provider);
            if (Collection is null) 
                throw new Exception("Collection null error!");
            IsLogicalDelete = InterfaceExistenceChecker.Check<T>(typeof(ILogicalDelete));
            IsCachable = InterfaceExistenceChecker.Check<T>(typeof(ICachable));
            if (IsCachable)
                CacheKey = $"{typeof(T).GetType().FullName}";
        }

        private static IDatabase CacheDb => RedisConnectorHelper.Db;
        protected bool IsLogicalDelete { get; }
        protected bool IsCachable { get; }
        protected string? CacheKey { get; }

        private void SetDatabaseAndCollection(IServiceProvider provider)
        {
            try
            {
                var configuration = provider.GetService<Configuration>() ?? throw new Exception("MongoDb credentials missing! (not injected)");
                var mongoDbConfigs = configuration.MongoDb ?? throw new Exception("MongoDb credentials missing! (credentials null)");
                var client = new MongoClient(mongoDbConfigs.ConnectionString);
                Database = client.GetDatabase(mongoDbConfigs.Database);
                Collection = Database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
            }
            catch (Exception e)
            {
                throw new Exception("MongoDb Connection Error! Exception : " + e);
            }
        }

        public async Task<T?> GetByIdAsync(U id, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (id is null)
                throw new Exception("Id is null!");
            return await SingleOrDefaultAsync(x => x.Id.Equals(id), includeLogicalDeleted, cancellationToken);
        }

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return selector == null ? await Collection.AsQueryable().SingleOrDefaultAsync(cancellationToken) : await Collection.Find(selector).SingleOrDefaultAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return null;
            return selector == null ? cachedData.SingleOrDefault() : cachedData.SingleOrDefault(selector);
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return selector == null ? await Collection.AsQueryable().FirstOrDefaultAsync(cancellationToken) : await Collection.Find(selector).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return null;
            return selector == null ? cachedData.FirstOrDefault() : cachedData.FirstOrDefault(selector);
        }
        public async Task<ICollection<T>> WhereAsync(Expression<Func<T, bool>> selector, bool includeLogicalDeleted = false, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.Find(selector).TSortBy(sorts).TPaginate(pagination, sorts).ToListAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return Enumerable.Empty<T>().ToList();
            return cachedData.SortBy(sorts).Paginate(pagination).ToList();
        }
        public async Task<ICollection<T>> GetAllAsync(bool includeLogicalDeleted = false, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.Find(x => true).TSortBy(sorts).TPaginate(pagination, sorts).ToListAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return Enumerable.Empty<T>().ToList();
            return cachedData.SortBy(sorts).Paginate(pagination).ToList();
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.CountDocumentsAsync(selector, cancellationToken: cancellationToken);

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return 0;
            return selector == null ? cachedData.Count() : cachedData.Count(selector);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return selector == null ? await Collection.AsQueryable().AnyAsync(cancellationToken) : await Collection.CountDocumentsAsync(selector, cancellationToken: cancellationToken) > 0;

            var cachedData = await GetCachedDataAsync(includeLogicalDeleted);
            if (cachedData is null)
                return false;
            return selector == null ? cachedData.Any() : cachedData.Any(selector);
        }
        public async Task<T> InsertOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.SetUpdatedAndCreatedDate();

            var options = new InsertOneOptions { BypassDocumentValidation = false };
            await Collection.InsertOneAsync(entity, options, cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheIfCachableAsync();

            return entity;
        }
        public async Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.SetUpdatedAndCreatedDate();
            }

            var options = new InsertManyOptions { BypassDocumentValidation = false };
            await Collection.InsertManyAsync(entities, options, cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheIfCachableAsync();

            return entities;
        }
        public async Task UpdateOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.SetUpdatedDate();

            await Collection.FindOneAndReplaceAsync(x => x.Id.Equals(entity.Id), entity, cancellationToken: cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheIfCachableAsync();
        }
        public async Task UpdateManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.SetUpdatedDate();
                await Collection.FindOneAndReplaceAsync(x => x.Id.Equals(entity.Id), entity, cancellationToken: cancellationToken);
            }

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheIfCachableAsync();
        }
        public async Task DeleteOneAsync(U id, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            var entity = await Collection.FindAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken) ?? throw new Exception("Not found an entity with given id!");

            if (IsLogicalDelete)
                await Collection.FindOneAndUpdateAsync(x => x.Id.Equals(id), Builders<T>.Update.Set(x => (x as ILogicalDelete).Deleted, true), cancellationToken: cancellationToken);
            else
                await Collection.DeleteOneAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheIfCachableAsync();
        }
        public async Task DeleteManyAsync(IEnumerable<U> ids, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            if (IsLogicalDelete)
                await Collection.FindOneAndUpdateAsync(x => ids.Contains(x.Id), Builders<T>.Update.Set(x => (x as ILogicalDelete).Deleted, true), cancellationToken: cancellationToken);
            else
                await Collection.DeleteManyAsync(x => ids.Contains(x.Id), cancellationToken: cancellationToken);

            if (unitOfWork is not null)
                unitOfWork.Committed += async (s, e) => await UpdateCacheIfCachableAsync(s, new UpdateCacheEventArgs { CancellationToken = cancellationToken });
            else
                await UpdateCacheIfCachableAsync();
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
        private async Task UpdateCacheIfCachableAsync()
        {
            if (!IsCachable) return;
            await CacheDb.StringSetAsync(CacheKey, Collection.ToJson());
        }
        private async Task UpdateCacheIfCachableAsync(object sender, UpdateCacheEventArgs e) => await UpdateCacheIfCachableAsync();
    }
    public class UpdateCacheEventArgs : EventArgs
    {
        public CancellationToken CancellationToken { get; set; }
    }
}
