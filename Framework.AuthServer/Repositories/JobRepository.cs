using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer.Repositories;

public class JobRepository : EfCoreRepositoryBase<Job, AuthServerDbContext, int>, IJobRepository
{
    public JobRepository(AuthServerDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ICollection<Job>> GetJobsWithApplicantCountAsync(int page, int count, string? searchTerm = null, bool? active = null)
    {
        var query = DbContext.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Applicants)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(j => j.Title.Contains(searchTerm) || 
                                   j.Department.Contains(searchTerm) || 
                                   j.Location.Contains(searchTerm));
        }

        if (active.HasValue)
        {
            query = query.Where(j => j.Active == active.Value);
        }

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip(page * count)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Job?> GetJobWithDetailsAsync(int id)
    {
        return await DbContext.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Applicants)
                .ThenInclude(a => a.AssignedTo)
            .Include(j => j.Applicants)
                .ThenInclude(a => a.Documents)
            .Include(j => j.Applicants)
                .ThenInclude(a => a.Interviews)
                    .ThenInclude(i => i.Interviewer)
            .FirstOrDefaultAsync(j => j.Id == id);
    }
}
