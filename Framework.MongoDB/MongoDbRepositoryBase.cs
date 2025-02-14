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
using Framework.MongoDB.Extensions;
using Framework.Shared.Entities.Configurations;
using MongoDB.Driver.Linq;

namespace Framework.MongoDB
{
    public class MongoDbRepositoryBase<T, U> : IGenericRepositoryWithNonRelation<T, U> where T : class, IBaseEntity<U>
    {
        private IMongoDatabase? Database;
        protected IMongoCollection<T> Collection;
        private static IDatabase CacheDb => RedisConnectorHelper.Db;
        protected bool IsLogicalDelete { get; }
        protected bool IsCachable { get; }

        public MongoDbRepositoryBase(IServiceProvider provider)
        {
            SetDatabaseAndCollection(provider);
            if (Collection is null) 
                throw new Exception("Collection null error!");
            IsLogicalDelete = InterfaceExistenceChecker.Check<T>(typeof(ILogicalDelete));
            IsCachable = InterfaceExistenceChecker.Check<T>(typeof(ICachable));
        }

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

            if (!IsCachable)
                return await SingleOrDefaultAsync(x => x.Id.Equals(id), includeLogicalDeleted, cancellationToken);

            return await GetCacheByIdAsync(id, includeLogicalDeleted);
        }

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return selector == null ? await Collection.AsQueryable().SingleOrDefaultAsync(cancellationToken) : await Collection.Find(selector).SingleOrDefaultAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCacheAsync(includeLogicalDeleted);
            if (cachedData is null)
                return null;
            return selector == null ? cachedData.SingleOrDefault() : cachedData.SingleOrDefault(selector);
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return selector == null ? await Collection.AsQueryable().FirstOrDefaultAsync(cancellationToken) : await Collection.Find(selector).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCacheAsync(includeLogicalDeleted);
            if (cachedData is null)
                return null;
            return selector == null ? cachedData.FirstOrDefault() : cachedData.FirstOrDefault(selector);
        }
        public async Task<ICollection<T>> WhereAsync(Expression<Func<T, bool>> selector, bool includeLogicalDeleted = false, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.Find(selector).TSortBy(sorts).TPaginate(pagination, sorts).ToListAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCacheAsync(includeLogicalDeleted);
            if (cachedData is null)
                return Enumerable.Empty<T>().ToList();
            return cachedData.SortBy(sorts).Paginate(pagination).ToList();
        }
        public async Task<ICollection<T>> GetAllAsync(bool includeLogicalDeleted = false, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.Find(x => true).TSortBy(sorts).TPaginate(pagination, sorts).ToListAsync(cancellationToken: cancellationToken);

            var cachedData = await GetCacheAsync(includeLogicalDeleted);
            if (cachedData is null)
                return Enumerable.Empty<T>().ToList();
            return cachedData.SortBy(sorts).Paginate(pagination).ToList();
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return await Collection.CountDocumentsAsync(selector, cancellationToken: cancellationToken);

            var cachedData = await GetCacheAsync(includeLogicalDeleted);
            if (cachedData is null)
                return 0;
            return selector == null ? cachedData.Count() : cachedData.Count(selector);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default)
        {
            if (!IsCachable)
                return selector == null ? await Collection.AsQueryable().AnyAsync(cancellationToken) : await Collection.CountDocumentsAsync(selector, cancellationToken: cancellationToken) > 0;

            var cachedData = await GetCacheAsync(includeLogicalDeleted);
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
            {
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await UpsertCacheAsync(s, new UpsertCacheEventArgs<T>
                    {
                        Entities = [entity]
                    });
            }
            else
            {
                if (IsCachable)
                    await UpsertCacheAsync([entity]);
            }

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
            {
                if (IsCachable)
                    unitOfWork.Committed += async (s, e) => await UpsertCacheAsync(s, new UpsertCacheEventArgs<T>
                    {
                        Entities = entities
                    });
            }
            else
            {
                if (IsCachable)
                    await UpsertCacheAsync(entities);
            }


            return entities;
        }
        public async Task UpdateOneAsync(T entity, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            entity.SetUpdatedDate();

            await Collection.FindOneAndReplaceAsync(x => x.Id.Equals(entity.Id), entity, cancellationToken: cancellationToken);

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
                if (IsCachable)
                    await UpsertCacheAsync([entity]);
            }
        }
        public async Task UpdateManyAsync(IEnumerable<T> entities, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.SetUpdatedDate();
                await Collection.FindOneAndReplaceAsync(x => x.Id.Equals(entity.Id), entity, cancellationToken: cancellationToken);
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
                if (IsCachable)
                    await UpsertCacheAsync(entities);
            }
        }
        public async Task DeleteOneAsync(U id, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            var entity = await Collection.FindAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken) ?? throw new Exception("Not found an entity with given id!");

            if (IsLogicalDelete)
                await Collection.FindOneAndUpdateAsync(x => x.Id.Equals(id), Builders<T>.Update.Set(x => (x as ILogicalDelete).Deleted, true), cancellationToken: cancellationToken);
            else
                await Collection.DeleteOneAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken);

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
                if (IsCachable)
                    await DeleteCacheAsync([id]);
            }
        }
        public async Task DeleteManyAsync(IEnumerable<U> ids, IUnitOfWorkEvents? unitOfWork = null, CancellationToken cancellationToken = default)
        {
            if (IsLogicalDelete)
                await Collection.FindOneAndUpdateAsync(x => ids.Contains(x.Id), Builders<T>.Update.Set(x => (x as ILogicalDelete).Deleted, true), cancellationToken: cancellationToken);
            else
                await Collection.DeleteManyAsync(x => ids.Contains(x.Id), cancellationToken: cancellationToken);

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
                if (IsCachable)
                    await DeleteCacheAsync(ids.ToArray());
            }
        }
        private async Task<T?> GetCacheByIdAsync(U Id, bool includeLogicalDeleted)
        {
            RedisValue value = await CacheDb.StringGetAsync($"{typeof(T).FullName}:{Id}");
            if (value.IsNullOrEmpty)
                return null;
#pragma warning disable CS8604 // Possible null reference argument.
            var data = JsonSerializer.Deserialize<T>(value);
#pragma warning restore CS8604 // Possible null reference argument.
            if (IsLogicalDelete && !includeLogicalDeleted && (data as ILogicalDelete).Deleted)
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
                    if (deserializedObject != null)
                        dataList.Add(deserializedObject);
                }
            }
            if (IsLogicalDelete)
                return dataList.AsQueryable().WhereIf(IsLogicalDelete, $"{nameof(ILogicalDelete.Deleted)}={includeLogicalDeleted.ToString().ToLower()}");
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
