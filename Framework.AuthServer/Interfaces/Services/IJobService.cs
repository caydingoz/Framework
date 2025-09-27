using Framework.AuthServer.Dtos.JobService.Input;
using Framework.AuthServer.Dtos.JobService.Output;
using Microsoft.AspNetCore.Http;

namespace Framework.AuthServer.Interfaces.Services;

public interface IJobService
{
    Task<GetJobsOutput> GetJobsAsync(int page, int count, string? searchTerm = null, bool? active = null);
    Task<JobDto?> GetJobByIdAsync(int id);
    Task<bool> CreateJobAsync(CreateJobInput input, Guid createdById);
    Task<bool> UpdateJobAsync(UpdateJobInput input);
    Task<bool> DeleteJobAsync(int id);
    Task<bool> ApplyForJobAsync(int jobId, ApplyJobInput input, IFormFile? resumeFile);
    Task<GetApplicantsOutput> GetApplicantsAsync(int page, int count, int? jobId = null, int? status = null, string? searchTerm = null);
    Task<ApplicantDto?> GetApplicantByIdAsync(int id);
    Task<bool> UpdateApplicantStatusAsync(UpdateApplicantStatusInput input);
    Task<bool> HireApplicantAsync(int applicantId, Guid hiredById);
    Task<bool> CreateInterviewAsync(CreateInterviewInput input);
    Task<bool> CreateScorecardAsync(CreateScorecardInput input);
    Task<TimeToHireReportOutput> GetTimeToHireReportAsync(DateTime fromDate, DateTime toDate);
}
