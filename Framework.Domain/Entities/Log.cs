using Framework.Domain.Entites;
using MongoDB.Bson;

namespace Framework.Domain.Entities
{
    public class Log : BaseEntity<ObjectId>
    {
        public string? Message { get; set; }
    }
}
