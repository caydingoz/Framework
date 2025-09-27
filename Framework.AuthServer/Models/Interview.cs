using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models;

public class Interview : Entity<int>
{
    public int ApplicantId { get; set; }
    
    public Applicant Applicant { get; set; } = null!;
    
    public Guid InterviewerId { get; set; }
    
    public User Interviewer { get; set; } = null!;
    
    public DateTime ScheduledAt { get; set; }
    
    public int DurationMinutes { get; set; }
    
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Status { get; set; } = string.Empty; // Scheduled, Completed, Cancelled, Rescheduled
    
    public string Feedback { get; set; } = string.Empty;
    
    public ICollection<Scorecard> Scorecards { get; set; } = [];
}
