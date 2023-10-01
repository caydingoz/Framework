using Framework.Domain.Interfaces.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Framework.Domain.Entites
{
    public class BaseEntity<T> : IBaseEntity<T>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public T Id { get; set; }
    }
}
