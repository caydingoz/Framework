using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;

namespace Framework.AuthServer.Interfaces.Repositories;

public interface IJobRepository : IGenericRepository<Job, int>
{
    Task<ICollection<Job>> GetJobsWithApplicantCountAsync(int page, int count, string? searchTerm = null, bool? active = null);
    Task<Job?> GetJobWithDetailsAsync(int id);
}
