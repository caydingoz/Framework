using AutoMapper;
using Framework.AuthServer.Dtos.NotificationService.Input;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Mappers
{
    public class NotificationMapper : Profile
    {
        public NotificationMapper()
        {
            CreateMap<CreateNotificationInput, Notification>();
        }
    }
}
