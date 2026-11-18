using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Services.Data;

namespace Conectify.Services.Automatization.Mapper;

public class RuleProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        configuration.CreateMap<AddInputApiModel, InputPoint>(
            x => new InputPoint
            {
                Id = Guid.NewGuid(),
                RuleId = x.RuleId,
                Type = x.InputType,
                Index = x.Index,
            },
            nameof(InputPoint.PreviousRules), nameof(InputPoint.Rule));

        configuration.CreateMap<AddOutputApiModel, OutputPoint>(
            x => new OutputPoint
            {
                Id = Guid.NewGuid(),
                RuleId = x.RuleId,
                Index = x.Index,
            },
            nameof(OutputPoint.ContinousRules), nameof(OutputPoint.Rule));

        configuration.CreateMap<InputPoint, InputPointDTO>(
            x => new InputPointDTO
            {
                Id = x.Id,
                Index = x.Index,
                Type = x.Type,
            },
            nameof(InputPointDTO.Rule));

        configuration.CreateMap<InputPoint, InputApiModel>(x => new InputApiModel
        {
            Id = x.Id,
            Index = x.Index,
            Type = x.Type,
        });

        configuration.CreateMap<OutputPoint, OutputApiModel>(x => new OutputApiModel
        {
            Id = x.Id,
            Index = x.Index,
        });

        configuration.CreateMap<Rule, RuleDTO>(
            x => new RuleDTO
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ParametersJson = x.ParametersJson,
                RuleTypeId = x.RuleType,
                SourceSensorId = SharedDataService.ExtractSourceId(x.ParametersJson),
                Inputs = (x.InputConnectors ?? new List<InputPoint>()).Select(i => new InputPointDTO
                {
                    Id = i.Id,
                    Index = i.Index,
                    Type = i.Type,
                }).ToList(),
            },
            nameof(RuleDTO.Outputs), nameof(RuleDTO.RuleBehaviour), nameof(RuleDTO.OutputValue), nameof(RuleDTO.TriggerOnValue));

        configuration.CreateMap<RuleDTO, Rule>(
            x => new Rule
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ParametersJson = x.ParametersJson,
            },
            nameof(Rule.RuleType), nameof(Rule.X), nameof(Rule.Y), nameof(Rule.FunctionId), nameof(Rule.Function), nameof(Rule.OutputConnectors), nameof(Rule.InputConnectors));

        configuration.CreateMap<CreateRuleApiModel, Rule>(
            x => new Rule
            {
                ParametersJson = x.Parameters,
                RuleType = x.BehaviourId,
                X = x.X,
                Y = x.Y,
            },
            nameof(Rule.Id), nameof(Rule.Name), nameof(Rule.Description), nameof(Rule.FunctionId), nameof(Rule.Function), nameof(Rule.OutputConnectors), nameof(Rule.InputConnectors));

        configuration.CreateMap<Rule, GetRuleApiModel>(x => new GetRuleApiModel
        {
            Id = x.Id,
            X = x.X,
            Y = x.Y,
            PropertyJson = x.ParametersJson,
            Name = x.Name,
            Description = x.Description,
            BehaviourId = x.RuleType,
            FunctionId = x.FunctionId,
            Inputs = x.InputConnectors.Select(i => new InputApiModel
            {
                Id = i.Id,
                Index = i.Index,
                Type = i.Type,
            }),
            Outputs = x.OutputConnectors.Select(o => new OutputApiModel
            {
                Id = o.Id,
                Index = o.Index,
            }),
        });

        configuration.CreateMap<RuleConnector, ConnectionApiModel>(x => new ConnectionApiModel
        {
            SourceId = x.SourceRuleId,
            DestinationId = x.TargetRuleId,
        });

        configuration.CreateMap<ConnectionApiModel, RuleConnector>(
            x => new RuleConnector
            {
                SourceRuleId = x.SourceId,
                TargetRuleId = x.DestinationId,
            },
            nameof(RuleConnector.SourceRule), nameof(RuleConnector.TargetRule));
    }
}
