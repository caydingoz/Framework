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
public class ApplicantController : BaseController
{
    private readonly IJobService _jobService;
    private readonly ILogger<ApplicantController> _logger;

    public ApplicantController(IJobService jobService, ILogger<ApplicantController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = OperationNames.Applicant + PermissionAccessTypes.ReadAccess)]
    public async Task<GeneralResponse<GetApplicantsOutput>> GetApplicantsAsync(
        [FromQuery] int page = 0, 
        [FromQuery] int count = 10, 
        [FromQuery] int? jobId = null, 
        [FromQuery] int? status = null, 
        [FromQuery] string? search = null)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            return await _jobService.GetApplicantsAsync(page, count, jobId, status, search);
        });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = OperationNames.Applicant + PermissionAccessTypes.ReadAccess)]
    public async Task<GeneralResponse<ApplicantDto?>> GetApplicantByIdAsync(int id)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            return await _jobService.GetApplicantByIdAsync(id);
        });
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = OperationNames.Applicant + PermissionAccessTypes.WriteAccess)]
    public async Task<GeneralResponse<bool>> UpdateApplicantStatusAsync(int id, UpdateApplicantStatusInput input)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            input.Id = id;
            return await _jobService.UpdateApplicantStatusAsync(input);
        });
    }

    [HttpPost("{id}/hire")]
    [Authorize(Policy = OperationNames.Applicant + PermissionAccessTypes.WriteAccess)]
    public async Task<GeneralResponse<bool>> HireApplicantAsync(int id)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            var hiredById = GetCurrentUserId();
            return await _jobService.HireApplicantAsync(id, hiredById);
        });
    }

    [HttpPost("{id}/interviews")]
    [Authorize(Policy = OperationNames.Applicant + PermissionAccessTypes.WriteAccess)]
    public async Task<GeneralResponse<bool>> CreateInterviewAsync(int id, CreateInterviewInput input)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            input.ApplicantId = id;
            return await _jobService.CreateInterviewAsync(input);
        });
    }

    [HttpPost("{id}/scorecards")]
    [Authorize(Policy = OperationNames.Applicant + PermissionAccessTypes.WriteAccess)]
    public async Task<GeneralResponse<bool>> CreateScorecardAsync(int id, CreateScorecardInput input)
    {
        return await WithLoggingGeneralResponseAsync(async () =>
        {
            // Note: In a real application, you would need to get the interview ID from the input or context
            // For now, we'll assume the input contains the interview ID
            return await _jobService.CreateScorecardAsync(input);
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
