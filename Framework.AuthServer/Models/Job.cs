using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models;

public class Job : Entity<int>, ILogicalDelete
{
    [StringLength(200)]
    public required string Title { get; set; }
    
    [StringLength(100)]
    public required string Department { get; set; }
    
    [StringLength(100)]
    public required string Location { get; set; }
    
    [StringLength(50)]
    public required string EmploymentType { get; set; } // Full-time, Part-time, Contract
    
    public required string Description { get; set; }
    
    public required string Responsibilities { get; set; }
    
    public required string Requirements { get; set; }
    
    public string[] Tags { get; set; } = [];
    
    public bool Active { get; set; } = true;
    
    public DateTime? PostedAt { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    
    public Guid CreatedById { get; set; }
    
    public User CreatedBy { get; set; } = null!;
    
    public string? ExtraFields { get; set; } // JSON for custom form fields config
    
    public bool IsDeleted { get; set; }
    
    public ICollection<Applicant> Applicants { get; set; } = [];
}
