namespace Framework.AuthServer.Dtos.JobService.Output;

public class TimeToHireReportOutput
{
    public List<TimeToHireData> Data { get; set; } = [];
    public decimal AverageTimeToHire { get; set; }
    public int TotalHired { get; set; }
    public int TotalApplications { get; set; }
}

public class TimeToHireData
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public DateTime? HiredAt { get; set; }
    public int? DaysToHire { get; set; }
}
