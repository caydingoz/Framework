namespace Framework.Shared.Entities
{
    public class Configuration
    {
        public JWTConfiguration? JWT { get; set; } = null;
        public MongoDbConfiguration? MongoDb { get; set; } = null;
        public EFConfiguration? EF { get; set; } = null;
    }
}
