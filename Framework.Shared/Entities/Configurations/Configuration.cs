namespace Framework.Shared.Entities.Configurations
{
    public class Configuration
    {
        public JWTConfiguration? JWT { get; set; } = null;
        public MongoDbConfiguration? MongoDb { get; set; } = null;
        public EFConfiguration? EF { get; set; } = null;
        public RedisConfiguration? Redis { get; set; } = null;
    }
}
