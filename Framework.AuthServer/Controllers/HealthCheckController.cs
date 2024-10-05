using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.HealthCheckService.Output;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Framework.Shared.Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class HealthCheckController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<HealthCheckController> Logger;
        private readonly HealthCheckService HealthCheckService;

        public HealthCheckController(
            Configuration configuration,
            ILogger<HealthCheckController> logger,
            HealthCheckService healthCheckService
            )
        {
            Configuration = configuration;
            Logger = logger;
            HealthCheckService = healthCheckService;
        }

        [HttpGet]
        [Authorize(Policy = OperationNames.SystemOperations + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<SeviceStatusesOutput>> GetSeviceStatusesAsync()
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var res = new SeviceStatusesOutput
                {
                    Services = new List<ServiceStatusOutput>()
                };

                string[] serviceNames = ["Sql Server", "Redis"];

                var healthReport = await HealthCheckService.CheckHealthAsync();

                foreach (var serviceName in serviceNames)
                {
                    var serviceStatus = healthReport.Entries.FirstOrDefault(e => e.Key == serviceName);
                    if (serviceStatus.Value.Status != HealthStatus.Healthy)
                    {
                        res.Services.Add(new ServiceStatusOutput
                        {
                            Name = serviceName,
                            IsAlive = false,
                            ErrorMessage = serviceStatus.Value.Exception?.Message ?? "Unknown error"
                        });
                    }
                    else
                    {
                        res.Services.Add(new ServiceStatusOutput
                        {
                            Name = serviceName,
                        });
                    }
                }

                return res;
            });
        }
    }
}