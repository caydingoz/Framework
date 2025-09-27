using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.JobService.Output;

public class GetApplicantsOutput : PageOutput
{
    public List<ApplicantDto> Applicants { get; set; } = [];
}
