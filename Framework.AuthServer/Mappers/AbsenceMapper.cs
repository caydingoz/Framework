using AutoMapper;
using Framework.AuthServer.Dtos.AbsenceService.Input;
using Framework.AuthServer.Dtos.AbsenceService.Output;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Mappers
{
    public class AbsenceMapper : Profile
    {
        public AbsenceMapper()
        {
            CreateMap<CreateAbsenceRequestInput, Absence>().ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Absence, AbsenceDTO>();
            CreateMap<Absence, GetAllAbsenceRequests>();
        }
    }
}
