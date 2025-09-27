namespace Framework.AuthServer.Dtos.JobService.Output;

public class InterviewDto
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public Guid InterviewerId { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ScorecardDto> Scorecards { get; set; } = [];
}
