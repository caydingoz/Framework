using AutoMapper;
using Framework.AuthServer.Dtos.JobService.Input;
using Framework.AuthServer.Dtos.JobService.Output;
using Framework.AuthServer.Enums;
using Framework.AuthServer.Models;

namespace Framework.AuthServer.Mappers;

public class ApplicantMapper : Profile
{
    public ApplicantMapper()
    {
        CreateMap<ApplyJobInput, Applicant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.JobId, opt => opt.Ignore())
            .ForMember(dest => dest.Job, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 1)) // Applied
            .ForMember(dest => dest.Score, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedToId, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.AppliedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.Interviews, opt => opt.Ignore());

        CreateMap<Applicant, ApplicantDto>()
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job != null ? src.Job.Title : string.Empty))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => ((ApplicantStatus)src.Status).ToString()))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}" : string.Empty))
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents))
            .ForMember(dest => dest.Interviews, opt => opt.MapFrom(src => src.Interviews));

        CreateMap<ApplicantDocument, ApplicantDocumentDto>();
        CreateMap<Interview, InterviewDto>()
            .ForMember(dest => dest.InterviewerName, opt => opt.MapFrom(src => src.Interviewer != null ? $"{src.Interviewer.FirstName} {src.Interviewer.LastName}" : string.Empty))
            .ForMember(dest => dest.Scorecards, opt => opt.MapFrom(src => src.Scorecards));
        CreateMap<Scorecard, ScorecardDto>()
            .ForMember(dest => dest.EvaluatorName, opt => opt.MapFrom(src => src.Evaluator != null ? $"{src.Evaluator.FirstName} {src.Evaluator.LastName}" : string.Empty));
    }
}
