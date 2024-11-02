using Framework.AuthServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.AbsenceService.Input
{
    public class CreateAbsenceRequestInput
    {
        [StringLength(1500)]
        public string Description { get; set; } = string.Empty;
        public required AbsenceTypes Type { get; set; }
        public required double Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
