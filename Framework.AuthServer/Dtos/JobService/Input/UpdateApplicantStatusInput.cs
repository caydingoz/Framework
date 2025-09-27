using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.JobService.Input;

public class UpdateApplicantStatusInput
{
    public int Id { get; set; }
    
    public int Status { get; set; }
    
    public Guid? AssignedToId { get; set; }
    
    public string? Note { get; set; }
}
