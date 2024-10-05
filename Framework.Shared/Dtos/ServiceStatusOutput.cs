namespace Framework.Shared.Dtos
{
    public class ServiceStatusOutput
    {
        public required string Name { get; set; }
        public bool IsAlive { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
