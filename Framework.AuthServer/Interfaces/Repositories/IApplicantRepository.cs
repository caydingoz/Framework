using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;

namespace Framework.AuthServer.Interfaces.Repositories;

public interface IApplicantRepository : IGenericRepository<Applicant, int>
{
    Task<ICollection<Applicant>> GetApplicantsWithDetailsAsync(int page, int count, int? jobId = null, int? status = null, string? searchTerm = null);
    Task<Applicant?> GetApplicantWithDetailsAsync(int id);
    Task<int> GetApplicantCountAsync(int? jobId = null, int? status = null, string? searchTerm = null);
}
