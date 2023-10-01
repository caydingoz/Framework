using Framework.Domain.Entites;

namespace Framework.Domain.Entities
{
    public class Log : Entity<string>
    {
        public string? Message { get; set; }
    }
}
