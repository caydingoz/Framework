using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.AbsenceService.Output
{
    public class GetUserAbsenceRequestsOutput : PageOutput
    {
        public ICollection<AbsenceDTO> Data { get; set; } = [];
    }
}
