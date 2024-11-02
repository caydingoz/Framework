using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.AbsenceService.Output
{
    public class GetAllAbsenceRequestsOutput : PageOutput
    {
        public ICollection<GetAllAbsenceRequests> Absences { get; set; } = [];
    }
    public class GetAllAbsenceRequests : AbsenceDTO
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
