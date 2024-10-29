using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.ActivityService.Input
{
    public class WorkItemDTO
    {
        public int Id { get; set; }
        [StringLength(1500)]
        public required string Title { get; set; }
    }
}
