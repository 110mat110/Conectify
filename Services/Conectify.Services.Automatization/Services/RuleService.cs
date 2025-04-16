using System.Diagnostics;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Services;

public class RuleService(IAutomatizationCache automatizationCache, IMapper mapper, AutomatizationDb database, IConnectorService connectorService, IDeviceData configuration, IServiceProvider services)
{
    public async Task<GetRuleApiModel> AddNewRule(Guid behaviourId, CancellationToken cancellationToken)
    {
        var behaviour = BehaviourFactory.GetRuleBehaviourByTypeId(behaviourId, services) ?? throw new Exception($"Behaviour {behaviourId} does not exist");

        var inputs = new List<InputPoint>();
        int inputIndex = 0;
        foreach (var input in behaviour.Inputs)
        {
            for (int i = 0; i < input.Item2.Def; i++)
                inputs.Add(new InputPoint()
                {
                    Index = inputIndex++,
                    Type = input.Item1,
                });
        }
        var outputs = new List<OutputPoint>();

        for (int i = 0; i < behaviour.Outputs.Def; i++)
            outputs.Add(new OutputPoint()
            {
                Index = i,
            });

        var rule = new Rule()
        {
            ParametersJson = "{}",
            RuleType = behaviourId,
            X = 0,
            Y = 0,
            InputConnectors = inputs,
            OutputConnectors = outputs
        };

        await AddExtraParamsToModel(rule, cancellationToken);
        _ = await automatizationCache.AddNewRule(rule, cancellationToken);

        return mapper.Map<GetRuleApiModel>(rule);
    }

    public async Task<Guid> AddInput(AddInputApiModel apiInput, CancellationToken ct = default)
    {
        var input = mapper.Map<InputPoint>(apiInput);
        var rule = await automatizationCache.GetRuleByIdAsync(input.RuleId, ct) ?? throw new ArgumentException("Rule does not exist!");

        var behaviour = BehaviourFactory.GetRuleBehaviourByTypeId(rule.RuleTypeId, services);

        if (!rule.CanAddInput(behaviour, input.Type))
        {
            throw new Exception("Cannot add new input");
        }

        var inputDTO = mapper.Map<InputPointDTO>(input);

        inputDTO.Rule = rule;
        input.RuleId = rule.Id;
        rule.Inputs.ToList().Add(inputDTO);

        await database.InputConnectors.AddAsync(input, ct);
        await database.SaveChangesAsync(ct);

        return input.Id;
    }

    public async Task<Guid> AddOutput(AddOutputApiModel apiOutput, CancellationToken ct = default)
    {
        var output = mapper.Map<OutputPoint>(apiOutput);
        var rule = await automatizationCache.GetRuleByIdAsync(output.RuleId, ct) ?? throw new ArgumentException("Rule does not exist!");

        var behaviour = BehaviourFactory.GetRuleBehaviourByTypeId(rule.RuleTypeId, services);
        if (!rule.CanAddOutput(behaviour))
        {
            throw new Exception("Cannot add new output");
        }

        var outputDTO = new OutputPointDTO(rule.Id, services);

        output.RuleId = rule.Id;
        rule.Outputs.ToList().Add(outputDTO);

        await database.OutputConnectors.AddAsync(output, ct);
        await database.SaveChangesAsync(ct);

        return output.Id;
    }

    public async Task<IEnumerable<GetRuleApiModel>> GetAllRules()
    {
        var result = await database.Set<Rule>().AsNoTracking().Include(i => i.InputConnectors).Include(i => i.OutputConnectors).ProjectTo<GetRuleApiModel>(mapper.ConfigurationProvider).ToListAsync();

        return result;
    }

    public async Task<IEnumerable<ConnectionApiModel>> GetAllConnections()
    {
        return await database.Set<RuleConnector>().ProjectTo<ConnectionApiModel>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<bool> EditRule(Guid ruleId, EditRuleApiModel apiRule, CancellationToken cancellationToken = default)
    {
        var rule = await database.Set<Rule>().FirstOrDefaultAsync(x => x.Id == ruleId, cancellationToken: cancellationToken);
        if (rule is null)
        {
            return false;
        }

        rule.ParametersJson = apiRule.Parameters;
        rule.X = apiRule.X;
        rule.Y = apiRule.Y;

        await AddExtraParamsToModel(rule, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);

        await automatizationCache.Reload(ruleId, cancellationToken);

        return true;
    }

    public async Task<bool> SetConnection(Guid sourceId, Guid destinationId, CancellationToken ct = default)
    {
        Debug.WriteLine("Starting set connection");
        var sourcePoint = await automatizationCache.GetOutputPointById(sourceId);
        var destinationPoint = await automatizationCache.GetInputPointById(destinationId);

        if (sourcePoint is null || destinationPoint is null)
        {
            return false;
        }

        if (automatizationCache.ConnectionExist(sourceId, destinationId))
        {
            var rule = await database.Set<RuleConnector>().FirstOrDefaultAsync(x => x.SourceRuleId == sourceId && x.TargetRuleId == destinationId, ct);

            if (rule is not null)
            {
                database.Set<RuleConnector>().Remove(rule);
                await database.SaveChangesAsync(ct);


                await automatizationCache.ReloadConnections();
                return true;
            }
        }

        var connection = new RuleConnector()
        {
            SourceRuleId = sourceId,
            TargetRuleId = destinationId,
        };

        await database.Set<RuleConnector>().AddAsync(connection, ct);
        await database.SaveChangesAsync(ct);

        await automatizationCache.ReloadConnections();
        return true;
    }

    public async Task<bool> RemoveConnection(Guid sourceId, Guid destinationId, CancellationToken ct = default)
    {
        var rule = await database.Set<RuleConnector>().FirstOrDefaultAsync(x => x.SourceRuleId == sourceId && x.TargetRuleId == destinationId, ct);

        if (rule is null)
        {
            return false;
        }

        database.Set<RuleConnector>().Remove(rule);
        await database.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> AddCustomInput(AddActuatorApiModel actuator, CancellationToken cancellationToken = default)
    {
        var apiSensor = new ApiSensor()
        {
            Id = Guid.NewGuid(),
            Name = actuator.ActuatorName + "-Sensor",
            SourceDeviceId = configuration.Device.Id,
        };
        var apiActuator = new ApiActuator()
        {
            Id = Guid.NewGuid(),
            Name = actuator.ActuatorName,
            SensorId = apiSensor.Id,
            SourceDeviceId = configuration.Device.Id,
        };
        await connectorService.RegisterDevice(configuration.Device, [apiSensor], [apiActuator], cancellationToken);

        var rule = new Rule()
        {
            Id = Guid.NewGuid(),
            Name = apiActuator.Name,
            ParametersJson = "{\"SourceSensorId\" : \"" + apiActuator.Id + "\"}",
            RuleType = new UserInputRuleBehaviour(services).GetId(),
            Description = "Automatically created from UI",
        };

        await automatizationCache.AddNewRule(rule, cancellationToken);

        return true;
    }

    private async Task AddExtraParamsToModel(Rule? rule, CancellationToken cancellationToken)
    {
        if (rule is null) return;

        var behaviour = BehaviourFactory.GetRuleBehaviourByTypeId(rule.RuleType, services);

        if (behaviour is null) return;

        await behaviour.SetParameters(rule, cancellationToken);
    }

    public async Task<bool> Remove(Guid ruleId, CancellationToken ct)
    {
        var rule = await database.Rules.FirstOrDefaultAsync(x => x.Id == ruleId, ct);

        if (rule is null) return false;

        database.Rules.Remove(rule);
        await database.SaveChangesAsync(ct);
        await automatizationCache.Reload(ruleId, ct);

        return true;
    }
}
