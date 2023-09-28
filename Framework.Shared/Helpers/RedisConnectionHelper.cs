using StackExchange.Redis;

namespace Framework.Shared.Helpers
{
    public class RedisConnectorHelper
    {
        static bool EventListenerCofigured = false;
        private static void SetupListener()
        {
            if (!EventListenerCofigured)
            {
                EventListenerCofigured = true;
            }
            if (LazyConnection == null)
            {
                try
                {
                    LazyConnection = ConnectionMultiplexer.Connect("127.0.0.1");
                }
                catch
                {
                    LazyConnection = null;
                }
            }
        }

        private static ConnectionMultiplexer LazyConnection { get; set; } = null;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                SetupListener();
                return LazyConnection;
            }
        }

        public static IDatabase Db => Connection.GetDatabase();
    }
}