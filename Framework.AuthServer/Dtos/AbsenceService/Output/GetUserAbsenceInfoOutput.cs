using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.AbsenceService.Output
{
    public class GetUserAbsenceInfoOutput
    {
        public ICollection<AbsenceInfo> AbsenceInfos { get; set; } = [];
    }
    public class AbsenceInfo
    {
        public DateTime AnnualStart { get; set; }
        public DateTime AnnualEnd { get; set; }
        public int AnnualDay { get; set; }
        public double UsedDay { get; set; }
        public double RemainingDay { get; set; }
    }
}
