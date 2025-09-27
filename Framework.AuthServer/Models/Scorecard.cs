using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models;

public class Scorecard : Entity<int>
{
    public int InterviewId { get; set; }
    
    public Interview Interview { get; set; } = null!;
    
    public Guid EvaluatorId { get; set; }
    
    public User Evaluator { get; set; } = null!;
    
    public string Criteria { get; set; } = string.Empty; // JSON
    
    public decimal? TotalScore { get; set; }
    
    public string Notes { get; set; } = string.Empty;
}
