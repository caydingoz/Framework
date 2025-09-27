using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.JobService.Output;
using Framework.AuthServer.Interfaces.Services;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class ReportsController : BaseController
{
    private readonly IJobService _jobService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IJobService jobService, ILogger<ReportsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpGet("time-to-hire")]
    [Authorize(Policy = OperationNames.Report + PermissionAccessTypes.ReadAccess)]
    public async Task<GeneralResponse<TimeToHireReportOutput>> GetTimeToHireReportAsync(
        [FromQuery] DateTime from, 
        [FromQuery] DateTime to)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            return await _jobService.GetTimeToHireReportAsync(from, to);
        });
    }
}
