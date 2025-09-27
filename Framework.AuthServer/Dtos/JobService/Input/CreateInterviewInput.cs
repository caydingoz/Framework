using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.JobService.Input;

public class CreateInterviewInput
{
    public int ApplicantId { get; set; }
    
    public Guid InterviewerId { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    
    public int DurationMinutes { get; set; }
    
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Status { get; set; } = "Scheduled";
}
