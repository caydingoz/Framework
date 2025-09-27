using Framework.AuthServer.Enums;

namespace Framework.AuthServer.Dtos.JobService.Output;

public class ApplicantDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public string CoverLetter { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public string[] Tags { get; set; } = [];
    public string? CustomFields { get; set; }
    public bool ConsentKvkk { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? HiredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ApplicantDocumentDto> Documents { get; set; } = [];
    public List<InterviewDto> Interviews { get; set; } = [];
}
