using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.JobService.Input;

public class CreateScorecardInput
{
    public int InterviewId { get; set; }
    
    public Guid EvaluatorId { get; set; }
    
    public string Criteria { get; set; } = string.Empty; // JSON
    
    public decimal? TotalScore { get; set; }
    
    public string Notes { get; set; } = string.Empty;
}
