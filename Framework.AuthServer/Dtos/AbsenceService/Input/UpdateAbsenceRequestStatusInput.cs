using Framework.AuthServer.Enums;

namespace Framework.AuthServer.Dtos.AbsenceService.Input
{
    public class UpdateAbsenceRequestStatusInput
    {
        public int Id { get; set; }
        public AbsenceStatus Status { get; set; }
    }
}
