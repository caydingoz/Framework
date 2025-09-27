using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models;

public class ApplicantDocument : Entity<int>
{
    public int ApplicantId { get; set; }
    
    public Applicant Applicant { get; set; } = null!;
    
    [StringLength(255)]
    public required string Filename { get; set; }
    
    [StringLength(500)]
    public required string FileUrl { get; set; }
    
    [StringLength(50)]
    public required string FileType { get; set; }
    
    public DateTime UploadedAt { get; set; }
}
