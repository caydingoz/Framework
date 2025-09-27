namespace Framework.AuthServer.Dtos.JobService.Output;

public class ApplicantDocumentDto
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
