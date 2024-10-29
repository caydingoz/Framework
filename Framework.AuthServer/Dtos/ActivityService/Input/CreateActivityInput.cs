using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.ActivityService.Input
{
    public class CreateActivityInput
    {
        [StringLength(1500)]
        public required string Description { get; set; }
        public required int WorkItemId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
