using Framework.AuthServer.Dtos.ActivityService.Input;

namespace Framework.AuthServer.Dtos.ActivityService.Output
{
    public class GetActivityForCalendarOutput
    {
        public ICollection<ActivityDTO> Activities { get; set; } = [];
    }
}
