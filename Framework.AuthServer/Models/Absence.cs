using Framework.AuthServer.Enums;
using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class Absence : Entity<int>
    {
        [StringLength(1500)]
        public required string Description { get; set; }
        public required AbsenceTypes Type { get; set; }
        public required AbsenceStatus Status { get; set; } = AbsenceStatus.Pending;
        public required Guid UserId { get; set; }
        public required double Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public User? User { get; set; }
    }
}
