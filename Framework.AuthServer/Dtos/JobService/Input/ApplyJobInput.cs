using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.JobService.Input;

public class ApplyJobInput
{
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
    public string Source { get; set; } = string.Empty;
    
    public string CoverLetter { get; set; } = string.Empty;
    
    public string? CustomFields { get; set; }
    
    public bool ConsentKvkk { get; set; }
}
