using Framework.Domain.Entites;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Framework.Domain.Entities
{
    public class Log : Entity<string>
    {
        public string? Message { get; set; }
    }
}
