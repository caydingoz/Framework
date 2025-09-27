using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.JobService.Output;

public class GetJobsOutput : PageOutput
{
    public List<JobDto> Jobs { get; set; } = [];
}
