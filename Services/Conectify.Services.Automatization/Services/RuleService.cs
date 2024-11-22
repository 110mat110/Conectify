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
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Services;

public class RuleService(IAutomatizationCache automatizationCache, IMapper mapper, AutomatizationDb database, IConnectorService connectorService, IDeviceData configuration, IServiceProvider services)
{
    public async Task<GetRuleApiModel> AddNewRule(Guid behaviourId, CancellationToken cancellationToken)
    {
        var behaviour = BehaviourFactory.GetRuleBehaviorByTypeId(behaviourId, services);

        var inputs = new List<InputPoint>();
        int inputIndex = 0;
        foreach (var input in behaviour.DefaultInputs)
        {
            for(int i =0; i< input.Item2; i++)
            inputs.Add(new InputPoint()
            {
                Index = inputIndex++,
                Type = input.Item1,
            });
        }
        var outputs = new List<OutputPoint>();

        for (int i = 0; i < behaviour.DefaultOutputs; i++)
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

        var savedRuleId = await automatizationCache.AddNewRule(rule, cancellationToken);

        return mapper.Map<GetRuleApiModel>(rule);
    }

    public async Task<Guid> AddInput(AddInputApiModel apiInput, CancellationToken ct = default)
    {
        var input = mapper.Map<InputPoint>(apiInput);
        var rule = await automatizationCache.GetRuleByIdAsync(input.RuleId) ?? throw new ArgumentException("Rule does not exist!");

        if (!rule.CanAddInput())
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
        var rule = await automatizationCache.GetRuleByIdAsync(output.RuleId) ?? throw new ArgumentException("Rule does not exist!");

        if (!rule.CanAddOutput())
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

        if (rule.RuleType == new OutputRuleBehaviour(services).GetId())
        {
            var id = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { DestinationId = Guid.Empty })?.DestinationId;
            if (id is null || id == Guid.Empty)
                return;

            var actuator = await connectorService.LoadActuator(id!.Value, cancellationToken);
            if(actuator is null)
            {
                return;
            }
            rule.ParametersJson = JsonConvert.SerializeObject(new { DestinationId = actuator.Id, actuator.Name });

            rule.Name = actuator.Name;
            rule.Description = actuator.Name;
        }

        if (rule.RuleType == new UserInputRuleBehaviour(services).GetId())
        {
            var id = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { SourceActuatorId = Guid.Empty })?.SourceActuatorId;
            if (id is null || id == Guid.Empty)
                return;

            var actuator = await connectorService.LoadActuator(id!.Value, cancellationToken);
            if (actuator is null)
            {
                return;
            }
            rule.ParametersJson = JsonConvert.SerializeObject(new { SourceActuatorId = actuator.Id, actuator.Name });

            rule.Name = actuator.Name;
            rule.Description = actuator.Name;
        }

        if (rule.RuleType == new InputRuleBehaviour(services).GetId())
        {
            var behaviour = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { SourceSensorId = Guid.Empty, Name = string.Empty, Event = string.Empty });
            if (behaviour is null || behaviour.SourceSensorId == Guid.Empty)
                return;

            var sensor = await connectorService.LoadSensor(behaviour.SourceSensorId, cancellationToken);
            if (sensor is null)
            {
                return;
            }
            rule.ParametersJson = JsonConvert.SerializeObject(new { SourceSensorId = sensor.Id, sensor.Name, behaviour.Event });

            rule.Name = sensor.Name;
            rule.Description = sensor.Name;
        }
    }
}
