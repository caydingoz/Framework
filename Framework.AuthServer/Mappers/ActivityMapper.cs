using AutoMapper;
using Framework.AuthServer.Dtos.ActivityService.Input;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Mappers
{
    public class ActivityMapper : Profile
    {
        public ActivityMapper()
        {
            CreateMap<CreateActivityInput, Activity>().ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<UpdateActivityInput, Activity>().ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Activity, ActivityDTO>();
            CreateMap<WorkItem, WorkItemDTO>();
        }
    }
}
