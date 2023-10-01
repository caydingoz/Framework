using Framework.Domain.Interfaces.Entities;

namespace Framework.Domain.Extensions
{
    public static class EntityExtensions
    {
        public static void SetUpdatedDate<T>(this T entity)
        {
            var now = DateTime.UtcNow;
            if (entity is IUpdated hasUpdated)
            {
                hasUpdated.UpdatedAt = now;
            }
        }
        public static void SetUpdatedAndCreatedDate<T>(this T entity)
        {
            var now = DateTime.UtcNow;
            if (entity is ICreated hasCreated)
            {
                hasCreated.CreatedAt = now;
            }
            if (entity is IUpdated hasUpdated)
            {
                hasUpdated.UpdatedAt = now;
            }
        }
    }
}
