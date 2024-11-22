using AutoMapper;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Shared.Services.Data;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Mapper;

public class RuleProfile : Profile
{
    public RuleProfile()
    {
        this.CreateMap<AddInputApiModel, InputPoint>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
            .ForMember(x => x.PreviousRules, opt => opt.Ignore())
            .ForMember(x => x.Rule, opt => opt.Ignore());

        this.CreateMap<AddOutputApiModel, OutputPoint>()
    .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
    .ForMember(x => x.ContinousRules, opt => opt.Ignore())
    .ForMember(x => x.Rule, opt => opt.Ignore());

        this.CreateMap<InputPoint, InputPointDTO>()
            .ForMember(x => x.Rule, opt => opt.Ignore())
            .ForMember(x => x.AutomatisationValue, opt => opt.Ignore());

        this.CreateMap<InputPoint, InputApiModel>();
        this.CreateMap<OutputPoint, OutputApiModel>();


        this.CreateMap<OutputPoint, OutputPointDTO>();


        this.CreateMap<Rule, RuleDTO>()
            .ForMember(x => x.RuleTypeId, opt => opt.MapFrom(x => x.RuleType))
            .ForMember(x => x.OutputValue, opt => opt.Ignore())
            .ForMember(x => x.SourceSensorId, opt => opt.MapFrom(x => SharedDataService.ExtractSourceId(x.ParametersJson)))
            .ForMember(x => x.Inputs, opt => opt.MapFrom(x => x.InputConnectors))
                        .ForMember(x => x.Outputs, opt => opt.Ignore());


        this.CreateMap<RuleDTO, Rule>()
            .ForMember(x => x.X, opt => opt.Ignore())
            .ForMember(x => x.Y, opt => opt.Ignore())
            .ForMember(x => x.OutputConnectors, opt => opt.Ignore())
            .ForMember(x => x.RuleType, opt => opt.Ignore())
            .ForMember(x => x.InputConnectors, opt => opt.Ignore());

        this.CreateMap<CreateRuleApiModel, Rule>()
            .ForMember(x => x.ParametersJson, opt => opt.MapFrom(x => x.Parameters))
            .ForMember(x => x.RuleType, opt => opt.MapFrom(x => x.BehaviourId))
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.InputConnectors, opt => opt.Ignore())
            .ForMember(x => x.Name, opt => opt.Ignore())
            .ForMember(x => x.OutputConnectors, opt => opt.Ignore())
            .ForMember(x => x.Description, opt => opt.Ignore());

        this.CreateMap<Rule, GetRuleApiModel>()
            .ForMember(x => x.PropertyJson, opt => opt.MapFrom(x => x.ParametersJson))
            .ForMember(x => x.BehaviourId, opt => opt.MapFrom(x => x.RuleType))
            .ForMember(x => x.Inputs, opt => opt.MapFrom(x => x.InputConnectors))
            .ForMember(x => x.Outputs, opt => opt.MapFrom(x => x.OutputConnectors));


        this.CreateMap<RuleConnector, ConnectionApiModel>()
            .ForMember(x => x.SourceId, opt => opt.MapFrom(x => x.SourceRuleId))
            .ForMember(x => x.DestinationId, opt => opt.MapFrom(x => x.TargetRuleId))
            .ReverseMap();
    }
}
