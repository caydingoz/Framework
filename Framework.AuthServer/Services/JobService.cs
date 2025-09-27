using AutoMapper;
using Framework.AuthServer.Dtos.JobService.Input;
using Framework.AuthServer.Dtos.JobService.Output;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Interfaces.Services;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Microsoft.AspNetCore.Http;

namespace Framework.AuthServer.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicantRepository _applicantRepository;
    private readonly IGenericRepository<ApplicantDocument, int> _applicantDocumentRepository;
    private readonly IGenericRepository<Interview, int> _interviewRepository;
    private readonly IGenericRepository<Scorecard, int> _scorecardRepository;
    private readonly IMapper _mapper;

    public JobService(
        IJobRepository jobRepository,
        IApplicantRepository applicantRepository,
        IGenericRepository<ApplicantDocument, int> applicantDocumentRepository,
        IGenericRepository<Interview, int> interviewRepository,
        IGenericRepository<Scorecard, int> scorecardRepository,
        IMapper mapper)
    {
        _jobRepository = jobRepository;
        _applicantRepository = applicantRepository;
        _applicantDocumentRepository = applicantDocumentRepository;
        _interviewRepository = interviewRepository;
        _scorecardRepository = scorecardRepository;
        _mapper = mapper;
    }

    public async Task<GetJobsOutput> GetJobsAsync(int page, int count, string? searchTerm = null, bool? active = null)
    {
        var jobs = await _jobRepository.GetJobsWithApplicantCountAsync(page, count, searchTerm, active);
        var totalCount = await _jobRepository.CountAsync(j => 
            (string.IsNullOrEmpty(searchTerm) || j.Title.Contains(searchTerm) || j.Department.Contains(searchTerm) || j.Location.Contains(searchTerm)) &&
            (!active.HasValue || j.Active == active.Value));

        var jobDtos = _mapper.Map<List<JobDto>>(jobs);

        return new GetJobsOutput
        {
            Jobs = jobDtos,
            TotalCount = totalCount
        };
    }

    public async Task<JobDto?> GetJobByIdAsync(int id)
    {
        var job = await _jobRepository.GetJobWithDetailsAsync(id);
        if (job == null)
        {
            return null;
        }

        return _mapper.Map<JobDto>(job);
    }

    public async Task<bool> CreateJobAsync(CreateJobInput input, Guid createdById)
    {
        var job = _mapper.Map<Job>(input);
        job.CreatedById = createdById;
        job.PostedAt = DateTime.UtcNow;

        await _jobRepository.InsertOneAsync(job);
        return true;
    }

    public async Task<bool> UpdateJobAsync(UpdateJobInput input)
    {
        var job = await _jobRepository.GetByIdAsync(input.Id) ?? throw new Exception("Job not found");

        _mapper.Map(input, job);
        await _jobRepository.UpdateOneAsync(job);
        return true;
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id) ?? throw new Exception("Job not found");

        job.IsDeleted = true;
        job.Active = false;
        job.ClosedAt = DateTime.UtcNow;
        await _jobRepository.UpdateOneAsync(job);
        return true;
    }

    public async Task<bool> ApplyForJobAsync(int jobId, ApplyJobInput input, IFormFile? resumeFile)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null || !job.Active)
            throw new Exception("Job not found or not active");

        var applicant = _mapper.Map<Applicant>(input);
        applicant.JobId = jobId;

        if (resumeFile != null)
        {
            // In a real application, you would save the file to a file storage service
            // For now, we'll just store the filename
            applicant.ResumeUrl = resumeFile.FileName;
        }

        await _applicantRepository.InsertOneAsync(applicant);

        // Save resume file if provided
        if (resumeFile != null)
        {
            var document = new ApplicantDocument
            {
                ApplicantId = applicant.Id,
                Filename = resumeFile.FileName,
                FileUrl = $"uploads/resumes/{applicant.Id}/{resumeFile.FileName}",
                FileType = resumeFile.ContentType,
                UploadedAt = DateTime.UtcNow
            };
            await _applicantDocumentRepository.InsertOneAsync(document);
        }

        return true;
    }

    public async Task<GetApplicantsOutput> GetApplicantsAsync(int page, int count, int? jobId = null, int? status = null, string? searchTerm = null)
    {
        var applicants = await _applicantRepository.GetApplicantsWithDetailsAsync(page, count, jobId, status, searchTerm);
        var totalCount = await _applicantRepository.GetApplicantCountAsync(jobId, status, searchTerm);

        var applicantDtos = _mapper.Map<List<ApplicantDto>>(applicants);

        return new GetApplicantsOutput
        {
            Applicants = applicantDtos,
            TotalCount = totalCount
        };
    }

    public async Task<ApplicantDto?> GetApplicantByIdAsync(int id)
    {
        var applicant = await _applicantRepository.GetApplicantWithDetailsAsync(id);
        if (applicant == null)
        {
            return null;
        }

        return _mapper.Map<ApplicantDto>(applicant);
    }

    public async Task<bool> UpdateApplicantStatusAsync(UpdateApplicantStatusInput input)
    {
        var applicant = await _applicantRepository.GetByIdAsync(input.Id) ?? throw new Exception("Applicant not found");

        applicant.Status = input.Status;
        applicant.AssignedToId = input.AssignedToId;

        if (input.Status == 5 && !applicant.HiredAt.HasValue)
            applicant.HiredAt = DateTime.UtcNow;

        await _applicantRepository.UpdateOneAsync(applicant);
        return true;
    }

    public async Task<bool> HireApplicantAsync(int applicantId, Guid hiredById)
    {
        var applicant = await _applicantRepository.GetByIdAsync(applicantId);
        if (applicant == null)
        {
            throw new Exception("Applicant not found");
        }

        applicant.Status = 5; // Hired
        applicant.AssignedToId = hiredById;
        applicant.HiredAt = DateTime.UtcNow;

        await _applicantRepository.UpdateOneAsync(applicant);
        return true;
    }

    public async Task<bool> CreateInterviewAsync(CreateInterviewInput input)
    {
        var interview = _mapper.Map<Interview>(input);
        await _interviewRepository.InsertOneAsync(interview);
        return true;
    }

    public async Task<bool> CreateScorecardAsync(CreateScorecardInput input)
    {
        var scorecard = _mapper.Map<Scorecard>(input);
        await _scorecardRepository.InsertOneAsync(scorecard);
        return true;
    }

    public async Task<TimeToHireReportOutput> GetTimeToHireReportAsync(DateTime fromDate, DateTime toDate)
    {
        var hiredApplicants = await _applicantRepository.WhereAsync(a => 
            a.Status == 5 && // Hired
            a.HiredAt.HasValue &&
            a.HiredAt >= fromDate && 
            a.HiredAt <= toDate,
            includes: a => a.Job);

        var reportData = new List<TimeToHireData>();
        var totalDays = 0;

        foreach (var applicant in hiredApplicants)
        {
            if (applicant.HiredAt.HasValue)
            {
                var daysToHire = (int)(applicant.HiredAt.Value - applicant.AppliedAt).TotalDays;
                totalDays += daysToHire;

                reportData.Add(new TimeToHireData
                {
                    JobId = applicant.JobId,
                    JobTitle = applicant.Job?.Title ?? string.Empty,
                    ApplicantId = applicant.Id,
                    ApplicantName = $"{applicant.FirstName} {applicant.LastName}",
                    AppliedAt = applicant.AppliedAt,
                    HiredAt = applicant.HiredAt.Value,
                    DaysToHire = daysToHire
                });
            }
        }

        var hiredApplicantsWithDates = hiredApplicants.Where(a => a.HiredAt.HasValue).ToList();
        var averageTimeToHire = hiredApplicantsWithDates.Count > 0 ? (decimal)totalDays / hiredApplicantsWithDates.Count : 0;

        return new TimeToHireReportOutput
        {
            Data = reportData,
            AverageTimeToHire = averageTimeToHire,
            TotalHired = hiredApplicantsWithDates.Count,
            TotalApplications = (int)await _applicantRepository.CountAsync(a => a.AppliedAt >= fromDate && a.AppliedAt <= toDate)
        };
    }
}
