using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class WorkItem : Entity<int>
    {
        [StringLength(1500)]
        public required string Title { get; set; }
        public ICollection<Activity> Activities { get; set; } = [];
        public ICollection<User> Users { get; set; } = [];
    }
}
