using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models.ActivityService;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Services;

public class RuleService
{
    private readonly AutomatizationCache automatizationCache;
    private readonly IMapper mapper;
    private readonly ConectifyDb conectifyDb;
    private readonly IConnectorService connectorService;
    private readonly IDeviceData configuration;

    public RuleService(AutomatizationCache automatizationCache, IMapper mapper, ConectifyDb conectifyDb, IConnectorService connectorService, IDeviceData configuration)
    {
        this.automatizationCache = automatizationCache;
        this.mapper = mapper;
        this.conectifyDb = conectifyDb;
        this.connectorService = connectorService;
        this.configuration = configuration;
    }

    public async Task<Guid> AddNewRule(CreateRuleApiModel apiModel, CancellationToken cancellationToken)
    {
        var dbsModel = mapper.Map<Rule>(apiModel);

        return await automatizationCache.AddNewRule(dbsModel, cancellationToken);
    }

    public async Task<IEnumerable<GetRuleApiModel>> GetAllRules()
    {
        return await conectifyDb.Set<Rule>().Include(i => i.ContinuingRules).ProjectTo<GetRuleApiModel>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<IEnumerable<ConnectionApiModel>> GetAllConnections()
    {
        return await conectifyDb.Set<RuleConnector>().ProjectTo<ConnectionApiModel>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<bool> EditRule(Guid ruleId, EditRuleApiModel apiRule)
    {
        var rule = await conectifyDb.Set<Rule>().FirstOrDefaultAsync(x => x.Id == ruleId);
        if (rule == null)
        {
            return false;
        }

        rule.ParametersJson = apiRule.Parameters;
        rule.X = apiRule.X;
        rule.Y = apiRule.Y;
        await conectifyDb.SaveChangesAsync();

        await automatizationCache.Reload(ruleId);

        return true;
    }

    public async Task<bool> AddConnection(Guid sourceId, Guid destinationId)
    {
        var sourceRule = await automatizationCache.GetRuleByIdAsync(sourceId);
        var destinationRule = await automatizationCache.GetRuleByIdAsync(destinationId);

        if (sourceRule is null || destinationRule is null || sourceRule.NextRules.Contains(destinationId))
        {
            return false;
        }

        sourceRule.NextRules = sourceRule.NextRules.Append(destinationId);

        var ruleConnector = await conectifyDb.Set<RuleConnector>().FirstOrDefaultAsync(x => x.PreviousRuleId == sourceId && x.ContinuingRuleId == destinationId);
        if (ruleConnector != null)
        {
            return false;
        }

        await conectifyDb.Set<RuleConnector>().AddAsync(new RuleConnector() { ContinuingRuleId = destinationId, PreviousRuleId = sourceId });
        await conectifyDb.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveConnection(Guid sourceId, Guid destinationId)
    {
        var sourceRule = await automatizationCache.GetRuleByIdAsync(sourceId);
        var destinationRule = await automatizationCache.GetRuleByIdAsync(destinationId);

        if (sourceRule is null || destinationRule is null || !sourceRule.NextRules.Contains(destinationId))
        {
            return false;
        }

        var list = sourceRule.NextRules.ToList();
        list.Remove(destinationId);
        sourceRule.NextRules = list;

        var sourceRuleDb = await conectifyDb.Set<RuleConnector>().FirstOrDefaultAsync(x => x.PreviousRuleId == sourceId && x.ContinuingRuleId == destinationId);
        if (sourceRuleDb == null)
        {
            return false;
        }

        conectifyDb.Set<RuleConnector>().Remove(sourceRuleDb);
        await conectifyDb.SaveChangesAsync();

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
        await connectorService.RegisterDevice(configuration.Device, new List<ApiSensor>() { apiSensor }, new List<ApiActuator>() { apiActuator }, cancellationToken);

        var rule = new Rule()
        {
            Id = Guid.NewGuid(),
            Name = apiActuator.Name,
            ParametersJson = "{\"SourceSensorId\" : \"" + apiActuator.Id + "\"}",
            RuleType = new UserInputRuleBehaviour().GetId(),
            Description = "Automatically created from UI",
        };

        await automatizationCache.AddNewRule(rule, cancellationToken);

        return true;
    }
}
