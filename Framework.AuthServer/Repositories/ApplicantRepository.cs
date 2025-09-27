using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer.Repositories;

public class ApplicantRepository : EfCoreRepositoryBase<Applicant, AuthServerDbContext, int>, IApplicantRepository
{
    public ApplicantRepository(AuthServerDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ICollection<Applicant>> GetApplicantsWithDetailsAsync(int page, int count, int? jobId = null, int? status = null, string? searchTerm = null)
    {
        var query = DbContext.Applicants
            .Include(a => a.Job)
            .Include(a => a.AssignedTo)
            .Include(a => a.Documents)
            .Include(a => a.Interviews)
                .ThenInclude(i => i.Interviewer)
            .Include(a => a.Interviews)
                .ThenInclude(i => i.Scorecards)
                    .ThenInclude(s => s.Evaluator)
            .AsQueryable();

        if (jobId.HasValue)
        {
            query = query.Where(a => a.JobId == jobId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(a => a.FirstName.Contains(searchTerm) || 
                                   a.LastName.Contains(searchTerm) || 
                                   a.Email.Contains(searchTerm));
        }

        return await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip(page * count)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Applicant?> GetApplicantWithDetailsAsync(int id)
    {
        return await DbContext.Applicants
            .Include(a => a.Job)
            .Include(a => a.AssignedTo)
            .Include(a => a.Documents)
            .Include(a => a.Interviews)
                .ThenInclude(i => i.Interviewer)
            .Include(a => a.Interviews)
                .ThenInclude(i => i.Scorecards)
                    .ThenInclude(s => s.Evaluator)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<int> GetApplicantCountAsync(int? jobId = null, int? status = null, string? searchTerm = null)
    {
        var query = DbContext.Applicants.AsQueryable();

        if (jobId.HasValue)
        {
            query = query.Where(a => a.JobId == jobId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(a => a.FirstName.Contains(searchTerm) || 
                                   a.LastName.Contains(searchTerm) || 
                                   a.Email.Contains(searchTerm));
        }

        return await query.CountAsync();
    }
}
