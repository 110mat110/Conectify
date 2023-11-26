using AutoMapper;
using Conectify.Database.Models.Automatization;
using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Shared.Services.Data;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Mapper;

public class RuleProfile : Profile
{
    public RuleProfile()
    {
        this.CreateMap<Rule, RuleDTO>()
            .ForMember(x => x.Values, opt => opt.Ignore())
            .ForMember(x => x.NextRules, opt => opt.MapFrom(x => x.ContinuingRules.Select(x => x.ContinuingRuleId)))
            .ForMember(x => x.Parameters, opt => opt.MapFrom(x => x.SourceParameters.Select(x => x.SourceRuleId)))
            .ForMember(x => x.RuleTypeId, opt => opt.MapFrom(x => x.RuleType))
            .ForMember(x => x.OutputValue, opt => opt.Ignore())
            .ForMember(x => x.SourceSensorId, opt => opt.MapFrom(x => SharedDataService.ExtractSourceId(x.ParametersJson)))
            .ForMember(x => x.DestinationActuatorId, opt => opt.MapFrom(x => SharedDataService.ExtractDestinationId(x.ParametersJson)));

        this.CreateMap<RuleDTO, Rule>()
            .ForMember(x => x.X, opt => opt.Ignore())
            .ForMember(x => x.Y, opt => opt.Ignore())
            .ForMember(x => x.ContinuingRules, opt => opt.Ignore())
            .ForMember(x => x.RuleType, opt => opt.Ignore())
            .ForMember(x => x.TargetParameters, opt => opt.Ignore())
            .ForMember(x => x.SourceParameters, opt => opt.Ignore())
            .ForMember(x => x.PreviousRules, opt => opt.Ignore());

        this.CreateMap<CreateRuleApiModel, Rule>()
            .ForMember(x => x.ParametersJson, opt => opt.MapFrom(x => x.Parameters))
            .ForMember(x => x.RuleType, opt => opt.MapFrom(x => x.BehaviourId))
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.PreviousRules, opt => opt.Ignore())
            .ForMember(x => x.Name, opt => opt.Ignore())
            .ForMember(x => x.ContinuingRules, opt => opt.Ignore())
            .ForMember(x => x.TargetParameters, opt => opt.Ignore())
            .ForMember(x => x.SourceParameters, opt => opt.Ignore())
            .ForMember(x => x.Description, opt => opt.Ignore());

        this.CreateMap<Rule, GetRuleApiModel>()
            .ForMember(x => x.PropertyJson, opt => opt.MapFrom(x => x.ParametersJson))
            .ForMember(x => x.BehaviourId, opt => opt.MapFrom(x => x.RuleType))
            .ForMember(x => x.Targets, opt => opt.MapFrom(x => x.ContinuingRules.Select(x => x.ContinuingRuleId)))
            .ForMember(x => x.Parameters, opt => opt.MapFrom(x => x.SourceParameters.Select(x => x.SourceRuleId)));

        this.CreateMap<RuleConnector, ConnectionApiModel>()
            .ForMember(x => x.SourceId, opt => opt.MapFrom(x => x.PreviousRuleId))
            .ForMember(x => x.DestinationId, opt => opt.MapFrom(x => x.ContinuingRuleId))
            .ReverseMap();

        this.CreateMap<RuleParameter, ConnectionApiModel>()
            .ForMember(x => x.SourceId, opt => opt.MapFrom(x => x.SourceRuleId))
            .ForMember(x => x.DestinationId, opt => opt.MapFrom(x => x.TargetRuleId))
            .ReverseMap();
    }
}
