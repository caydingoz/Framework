using Framework.Shared.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Framework.MongoDB.Extensions
{
    public static class MongoDbExtension
    {
        public static IFindFluent<T, K> TSortBy<T, K>(this IFindFluent<T, K> query, ICollection<Sort>? sorts = null)
        {
            if (sorts is null || sorts.Count == 0)
                return query;

            var sortDoc = new BsonDocument();
            foreach (var sort in sorts)
                sortDoc.Add(new BsonElement(sort.Name, sort.Type == Shared.Enums.SortTypes.ASC ? 1 : -1));

            query.Sort(sortDoc);
            return query;
        }
        public static IFindFluent<T, K> TPaginate<T, K>(this IFindFluent<T, K> query, Pagination? pagination = null, ICollection<Sort>? sorts = null)
        {
            if (pagination is null)
                return query;

            if (sorts is null || sorts.Count == 0)
                query.Sort(new BsonDocument { { "_id", 1 } });

            query = query.Skip(pagination.Page * pagination.Count).Limit(pagination.Count);
            return query;
        }
    }
}
