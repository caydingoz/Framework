using AutoMapper;
using Framework.AuthServer.Dtos.NotificationService.Output;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Mappers
{
    public class NotificationMapper : Profile
    {
        public NotificationMapper()
        {
            CreateMap<NotificationDTO, Notification>().ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Notification, NotificationDTO>();
        }
    }
}
