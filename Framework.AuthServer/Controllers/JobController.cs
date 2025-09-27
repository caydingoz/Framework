using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.JobService.Input;
using Framework.AuthServer.Dtos.JobService.Output;
using Framework.AuthServer.Interfaces.Services;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Framework.AuthServer.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class JobController : BaseController
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobController> _logger;

    public JobController(IJobService jobService, ILogger<JobController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = OperationNames.Job + PermissionAccessTypes.ReadAccess)]
    public async Task<GeneralResponse<GetJobsOutput>> GetJobsAsync(
        [FromQuery] int page = 0, 
        [FromQuery] int count = 10, 
        [FromQuery] string? searchTerm = null, 
        [FromQuery] bool? active = null)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            return await _jobService.GetJobsAsync(page, count, searchTerm, active);
        });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = OperationNames.Job + PermissionAccessTypes.ReadAccess)]
    public async Task<GeneralResponse<JobDto?>> GetJobByIdAsync(int id)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            return await _jobService.GetJobByIdAsync(id);
        });
    }

    [HttpPost]
    [Authorize(Policy = OperationNames.Job + PermissionAccessTypes.WriteAccess)]
    public async Task<GeneralResponse<bool>> CreateJobAsync(CreateJobInput input)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            var userId = GetCurrentUserId();
            return await _jobService.CreateJobAsync(input, userId);
        });
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = OperationNames.Job + PermissionAccessTypes.WriteAccess)]
    public async Task<GeneralResponse<bool>> UpdateJobAsync(int id, UpdateJobInput input)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            input.Id = id;
            return await _jobService.UpdateJobAsync(input);
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = OperationNames.Job + PermissionAccessTypes.DeleteAccess)]
    public async Task<GeneralResponse<bool>> DeleteJobAsync(int id)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            return await _jobService.DeleteJobAsync(id);
        });
    }

    [HttpPost("{id}/apply")]
    [AllowAnonymous] // Allow anonymous job applications
    public async Task<GeneralResponse<bool>> ApplyForJobAsync(int id, ApplyJobInput input, IFormFile? resume)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            return await _jobService.ApplyForJobAsync(id, input, resume);
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}
