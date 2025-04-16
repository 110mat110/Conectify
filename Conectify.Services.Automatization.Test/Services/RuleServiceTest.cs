using System.Diagnostics;
using AutoMapper;
using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Mapper;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Library.Services;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Automatization.Test.Services;

public class RuleServiceTests
{
    private readonly AutomatizationDb dbContext;
    private readonly AutomatizationCache automatizationCache;
    private readonly IMapper mapper;
    private readonly IServiceProvider serviceProvider;
    private readonly ConnectorService connectorService;

    private Guid IncorrectBehaviourId = Guid.Parse("28ff4530-887b-48d1-a4fa-000000000000");

    public RuleServiceTests()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        dbContext = new AutomatizationDb(contextOptions);

        mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventProfile>();
            cfg.AddProfile<RuleProfile>();
            cfg.AddProfile<ActuatorProfile>();

        }).CreateMapper();
        connectorService = new ConnectorService(A.Fake<ILogger<ConnectorService>>(), new FakeConfig(), mapper, A.Fake<IHttpFactory>());

        var services = new ServiceCollection();
        services.AddTransient<IConnectorService>(services => connectorService);
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        serviceProvider = services.BuildServiceProvider();

        automatizationCache = new AutomatizationCache(serviceProvider, mapper,false);

    }

    [Fact]
    public async Task AddNewRule_SuccessfulAsync()
    {
        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);

        var behaviourId = new AndRuleBehaviour(default).GetId();
        var result = await ruleService.AddNewRule(behaviourId, default);

        Assert.NotNull(result);
        Assert.NotEqual(result.Id, Guid.Empty);
        Assert.Equal(result.BehaviourId, behaviourId);

        var dbRule = dbContext.Rules.FirstOrDefault(x => x.Id == result.Id);
        Assert.NotNull(dbRule);
        Assert.Equal(dbRule.Id, result.Id);
        Assert.Equal(dbRule.RuleType, behaviourId);

        var cachedRule = await automatizationCache.GetRuleByIdAsync(result.Id);
        Assert.NotNull(cachedRule);
        Assert.Equal(cachedRule.Id, result.Id);
        Assert.Equal(cachedRule.RuleBehaviour?.GetId(), behaviourId);
    }

    [Fact]
    public async Task AddNewRule_IncorrectId()
    {
        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);


        try
        {
            var result = await ruleService.AddNewRule(IncorrectBehaviourId, default);

        }
        catch (Exception ex)
        {
            Assert.Equal($"Cannot load behaviour {IncorrectBehaviourId}", ex.Message);
        }
    }

    [Fact]
    public async Task AddInput_Success()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
        });
        await dbContext.SaveChangesAsync();

        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);


        var input = await ruleService.AddInput(new Models.ApiModels.AddInputApiModel() { Index = 0, InputType = Models.Database.InputTypeEnum.Trigger, RuleId = ruleId });

        var dbRule = dbContext.Rules.FirstOrDefault(x => x.Id == ruleId);
        Assert.NotEmpty(dbRule.InputConnectors);
    }

    [Fact]
    public async Task AddInput_CannotAdd()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = new RunAtRuleBehaviour(default).GetId(),
        });
        await dbContext.SaveChangesAsync();


        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);

        try
        {
            await ruleService.AddInput(new Models.ApiModels.AddInputApiModel() { Index = 0, InputType = Models.Database.InputTypeEnum.Parameter, RuleId = ruleId });
        }
        catch (Exception ex)
        {
            Assert.Equal("Cannot add new input", ex.Message);
        }
    }

    [Fact]
    public async Task AddInput_IncorrectBehaviour()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = IncorrectBehaviourId,
        });
        await dbContext.SaveChangesAsync();

        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);

        try
        {
            var input = await ruleService.AddInput(new Models.ApiModels.AddInputApiModel() { Index = 0, InputType = Models.Database.InputTypeEnum.Trigger, RuleId = ruleId });
        }
        catch (Exception ex)
        {
            Assert.Equal($"Cannot load behaviour {IncorrectBehaviourId}", ex.Message);
        }
    }

    [Fact]
    public async Task AddOutput_Success()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
        });
        await dbContext.SaveChangesAsync();

        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);


        var input = await ruleService.AddOutput(new Models.ApiModels.AddOutputApiModel() { Index = 0, RuleId = ruleId });

        var dbRule = dbContext.Rules.FirstOrDefault(x => x.Id == ruleId);
        Assert.NotEmpty(dbRule.OutputConnectors);
    }

    [Fact]
    public async Task AddOutput_CannotAdd()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = new RunAtRuleBehaviour(default).GetId(),
        });
        await dbContext.SaveChangesAsync();


        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);

        try
        {
            await ruleService.AddOutput(new Models.ApiModels.AddOutputApiModel() { Index = 0, RuleId = ruleId });
            await ruleService.AddOutput(new Models.ApiModels.AddOutputApiModel() { Index = 1, RuleId = ruleId });
            await ruleService.AddOutput(new Models.ApiModels.AddOutputApiModel() { Index = 2, RuleId = ruleId });
        }
        catch (Exception ex)
        {
            Assert.Equal("Cannot add new input", ex.Message);
        }
    }

    [Fact]
    public async Task AddOutput_IncorrectBehaviour()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = IncorrectBehaviourId,
        });
        await dbContext.SaveChangesAsync();

        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);

        try
        {
            var input = await ruleService.AddOutput(new Models.ApiModels.AddOutputApiModel() { Index = 0, RuleId = ruleId });
        }
        catch (Exception ex)
        {
            Assert.Equal($"Cannot load behaviour {IncorrectBehaviourId}", ex.Message);
        }
    }

    [Fact]
    public async Task GetAllRules_ShouldLoadAllRules()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = IncorrectBehaviourId,
        });
        var ruleId2 = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId2,
            RuleType = new AndRuleBehaviour(default).GetId(),
        });
        await dbContext.SaveChangesAsync();

        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);
        var allRules = await ruleService.GetAllRules();

        Assert.NotEmpty(allRules);
        Assert.Contains(allRules, x => x.Id == ruleId);
        Assert.Contains(allRules, x => x.Id == ruleId2);
    }

    [Fact]
    public async Task EditRules_Success()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = Guid.NewGuid().ToString()
        });
        await dbContext.SaveChangesAsync();

        string newTestingJson = Guid.NewGuid().ToString();

        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);
        var editResult = await ruleService.EditRule(ruleId, new EditRuleApiModel() { BehaviourId = new RunAtRuleBehaviour(default).GetId(), Parameters = newTestingJson, Id = ruleId});

        Assert.True(editResult);
        var dbRule = dbContext.Rules.FirstOrDefault(x => x.Id == ruleId);
        Assert.NotNull(dbRule);
        Assert.Equal(dbRule.Id, ruleId);
        Assert.Equal(dbRule.ParametersJson, newTestingJson);

        var cachedRule = await automatizationCache.GetRuleByIdAsync(ruleId);
        Assert.NotNull(cachedRule);
        Assert.Equal(cachedRule.Id, ruleId);
        Assert.Equal(cachedRule.ParametersJson, newTestingJson);
    }

    [Fact]
    public async Task EditRules_FailedRuleInitialization_ShouldThrow() 
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = ruleId,
            RuleType = new InputRuleBehaviour(default).GetId(),
        });
        await dbContext.SaveChangesAsync();

        string invalidJson = "Invalid parameters!";

        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);
        try
        {
            var editResult = await ruleService.EditRule(ruleId, new EditRuleApiModel() { BehaviourId = new RunAtRuleBehaviour(default).GetId(), Parameters = invalidJson, Id = ruleId });
        }
        catch (Exception ex)
        {
            //Every rule should check their own parameters parsing. There is no default parser. 
            //This test only ensures, that exception will bubble up
            Assert.NotNull(ex);
            return;
        }

        Assert.True(false);
    }

    [Fact]
    public async Task EditRule_EditNonExistentRule_ShouldReturnFalse()
    {
        var ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);
        var editResult = await ruleService.EditRule(Guid.NewGuid(), new EditRuleApiModel());

        Assert.False(editResult);
    }

    [Fact]
    public async Task SetConnection_Success()
    {

        var inputId = Guid.NewGuid();
        var outputId = Guid.NewGuid();


        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
    .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
    .Options;
        var localDbContext = new AutomatizationDb(contextOptions);

        var localServices = new ServiceCollection();
        localServices.AddTransient<IConnectorService>(services => connectorService);
        localServices.AddScoped(services => new AutomatizationDb(contextOptions));
        var localServiceProvider = localServices.BuildServiceProvider();
        var automatizationCacheLocal = new AutomatizationCache(localServiceProvider, mapper, false);

        await localDbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = Guid.NewGuid(),
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = Guid.NewGuid().ToString(),
            InputConnectors = [new() { Id = inputId }]
        });
        await localDbContext.Rules.AddAsync(new Models.Database.Rule()
        {
            Id = Guid.NewGuid(),
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = Guid.NewGuid().ToString(),
            OutputConnectors = [new() { Id = outputId }]
        });
        await localDbContext.SaveChangesAsync();

        var ruleService = new RuleService(automatizationCacheLocal, mapper, localDbContext, connectorService, new FakeConfig(), localServiceProvider);
        var addedRules = await ruleService.SetConnection(outputId, inputId);

        Assert.True(addedRules);

        Assert.True(automatizationCacheLocal.ConnectionExist(outputId, inputId));

        Assert.NotNull(localDbContext.Set<RuleConnector>().Single(x => x.SourceRuleId == outputId && x.TargetRuleId == inputId));
    }

    private class FakeConfig : ConfigurationBase, IDeviceData
    {
        public FakeConfig() : base(A.Fake<IConfiguration>())
        {

        }

        public ApiDevice Device { get; set; }

        public IEnumerable<ApiSensor> Sensors { get; set; }

        public IEnumerable<ApiActuator> Actuators { get; set; }

        public IEnumerable<ApiPreference> Preferences { get; set; }

        public IEnumerable<MetadataServiceConnector> MetadataConnectors { get; set; }
    }
}
