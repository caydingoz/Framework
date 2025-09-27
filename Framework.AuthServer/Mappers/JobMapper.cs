using AutoMapper;
using Framework.AuthServer.Dtos.JobService.Input;
using Framework.AuthServer.Dtos.JobService.Output;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Mappers;

public class JobMapper : Profile
{
    public JobMapper()
    {
        CreateMap<CreateJobInput, Job>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Applicants, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateJobInput, Job>()
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Applicants, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<Job, JobDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : string.Empty))
            .ForMember(dest => dest.ApplicantCount, opt => opt.MapFrom(src => src.Applicants.Count));
    }
}
