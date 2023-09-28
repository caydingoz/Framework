using Framework.Domain.Interfaces.Entities;
using System.Security.Cryptography;

namespace Framework.Domain.Extensions
{

    public static class EntityExtensions
    {
        private static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();
        public static Guid NewSequentalGuid()
        {
            var randomBytes = new byte[10];
            RandomNumberGenerator.GetBytes(randomBytes);
            long timestamp = DateTime.UtcNow.Ticks / 10000L;

            byte[] timestampBytes = BitConverter.GetBytes(timestamp);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            byte[] guidBytes = new byte[16];
            Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
            Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);

            return new Guid(guidBytes);
        }

        public static T NewId<T>(T id)
        {
            if (typeof(T) != typeof(Guid) && id != null)
                return id;
            if (typeof(T) == typeof(Guid) && id != null && id.ToString() != Guid.Empty.ToString())
                return id;
            if (typeof(T) == typeof(Guid))
                return (T)Convert.ChangeType(NewSequentalGuid(), typeof(T));
            return default;
        }
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
