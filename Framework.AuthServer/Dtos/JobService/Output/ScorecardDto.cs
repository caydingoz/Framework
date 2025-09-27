namespace Framework.AuthServer.Dtos.JobService.Output;

public class ScorecardDto
{
    public int Id { get; set; }
    public int InterviewId { get; set; }
    public Guid EvaluatorId { get; set; }
    public string EvaluatorName { get; set; } = string.Empty;
    public string Criteria { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
