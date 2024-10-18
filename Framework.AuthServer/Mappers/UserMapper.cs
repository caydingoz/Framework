using AutoMapper;
using Framework.AuthServer.Dtos.UserService.Input;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<CreateUserInput, User>().ForMember(dest => dest.Password, opt => opt.MapFrom(src => src));
            CreateMap<UpdateUserInput, User>().ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
