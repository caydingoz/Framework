using Framework.AuthServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.AbsenceService.Output
{
    public class AbsenceDTO
    {
        public int Id { get; set; }
        [StringLength(1500)]
        public required string Description { get; set; }
        public required AbsenceTypes Type { get; set; }
        public required AbsenceStatus Status { get; set; }
        public required double Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
