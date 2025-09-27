using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.JobService.Input;

public class UpdateJobInput
{
    public int Id { get; set; }
    
    [StringLength(200)]
    public required string Title { get; set; }
    
    [StringLength(100)]
    public required string Department { get; set; }
    
    [StringLength(100)]
    public required string Location { get; set; }
    
    [StringLength(50)]
    public required string EmploymentType { get; set; }
    
    public required string Description { get; set; }
    
    public required string Responsibilities { get; set; }
    
    public required string Requirements { get; set; }
    
    public string[] Tags { get; set; } = [];
    
    public bool Active { get; set; } = true;
    
    public DateTime? PostedAt { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    
    public string? ExtraFields { get; set; }
}
