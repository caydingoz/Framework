using Framework.Shared.Entities.Configurations;
using StackExchange.Redis;

namespace Framework.Shared.Helpers
{
    public static class RedisConnectorHelper
    {
        private static void SetupListener()
        {
            if (LazyConnection == null)
            {
                try
                {
                    if (Configuration.Redis is null)
                        throw new Exception("Redis configuration null!");
                    LazyConnection = ConnectionMultiplexer.Connect(Configuration.Redis.ConnectionString);
                }
                catch
                {
                    LazyConnection = null;
                }
            }
        }
        
        private static ConnectionMultiplexer? LazyConnection { get; set; } = null;

        public static ConnectionMultiplexer? Connection
        {
            get
            {
                SetupListener();
                return LazyConnection;
            }
        }

        public static IDatabase Db => Connection.GetDatabase();

        public static Configuration Configuration { private get; set; }
    }
}