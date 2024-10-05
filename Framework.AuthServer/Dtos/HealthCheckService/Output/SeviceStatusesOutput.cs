using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.HealthCheckService.Output
{
    public class SeviceStatusesOutput
    {
        public ICollection<ServiceStatusOutput> Services { get; set; } = [];
    }
}
