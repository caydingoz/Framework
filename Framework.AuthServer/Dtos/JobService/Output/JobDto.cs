using Framework.AuthServer.Enums;

namespace Framework.AuthServer.Dtos.JobService.Output;

public class JobDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Responsibilities { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
    public bool Active { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string? ExtraFields { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ApplicantCount { get; set; }
}
