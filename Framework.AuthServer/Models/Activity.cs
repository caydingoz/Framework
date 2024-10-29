using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class Activity : Entity<int>
    {
        [StringLength(1500)]
        public required string Description { get; set; }
        public required int WorkItemId { get; set; }
        public required Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public User? User { get; set; }
        public WorkItem? WorkItem { get; set; }
    }
}
