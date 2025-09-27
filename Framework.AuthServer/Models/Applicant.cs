using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models;

public class Applicant : Entity<int>, ILogicalDelete
{
    public int JobId { get; set; }
    
    public Job Job { get; set; } = null!;
    
    [StringLength(100)]
    public required string FirstName { get; set; }
    
    [StringLength(100)]
    public required string LastName { get; set; }
    
    [StringLength(200)]
    public required string Email { get; set; }
    
    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Source { get; set; } = string.Empty; // career-site, linkedin, referral, manual
    
    [StringLength(500)]
    public string ResumeUrl { get; set; } = string.Empty;
    
    public string CoverLetter { get; set; } = string.Empty;
    
    public int Status { get; set; } // Enum: Applied, Screening, Interview, Offer, Hired, Rejected
    
    public decimal? Score { get; set; }
    
    public Guid? AssignedToId { get; set; }
    
    public User? AssignedTo { get; set; }
    
    public string[] Tags { get; set; } = [];
    
    public string? CustomFields { get; set; } // JSON
    
    public bool ConsentKvkk { get; set; }
    
    public DateTime AppliedAt { get; set; }
    
    public DateTime? HiredAt { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public ICollection<ApplicantDocument> Documents { get; set; } = [];
    
    public ICollection<Interview> Interviews { get; set; } = [];
}
