using AutoMapper;
using Conectify.Services.Automatization.Controllers;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Automatization.Test.Controllers;

public class RuleControllerTests
{
    private readonly AutomatizationDb dbContext;
    private readonly AutomatizationCache automatizationCache;
    private readonly IMapper mapper;
    private readonly IServiceProvider serviceProvider;
    private readonly ConnectorService connectorService;
    private readonly RuleService ruleService;
    private readonly RuleController controller;

    public RuleControllerTests()
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

        automatizationCache = new AutomatizationCache(serviceProvider, mapper, false);
        ruleService = new RuleService(automatizationCache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider);
        controller = new RuleController(ruleService);
    }

    [Fact]
    public async Task AddNewRule_ReturnsOkWithRule()
    {
        var behaviourId = new AndRuleBehaviour(default).GetId();
        var result = await controller.AddNewRule(behaviourId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var rule = Assert.IsType<GetRuleApiModel>(okResult.Value);
        Assert.NotEqual(Guid.Empty, rule.Id);
        Assert.Equal(behaviourId, rule.BehaviourId);
    }

    [Fact]
    public async Task GetAllRules_ReturnsOkWithRules()
    {
        await dbContext.Rules.AddAsync(new Rule
        {
            Id = Guid.NewGuid(),
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}"
        });
        await dbContext.SaveChangesAsync();

        var result = await controller.GetAllRules();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var rules = Assert.IsAssignableFrom<IEnumerable<GetRuleApiModel>>(okResult.Value);
        Assert.NotEmpty(rules);
    }

    [Fact]
    public async Task GetAllConnections_ReturnsConnections()
    {
        var outputId = Guid.NewGuid();
        var inputId = Guid.NewGuid();

        await dbContext.Set<RuleConnector>().AddAsync(new RuleConnector
        {
            SourceRuleId = outputId,
            TargetRuleId = inputId
        });
        await dbContext.SaveChangesAsync();

        var result = await controller.GetAllConnections();

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task EditRule_ValidRule_ReturnsOkWithTrue()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Rule
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}"
        });
        await dbContext.SaveChangesAsync();

        var editModel = new EditRuleApiModel
        {
            Id = ruleId,
            BehaviourId = new AndRuleBehaviour(default).GetId(),
            Parameters = "{\"updated\":true}",
            X = 50,
            Y = 100
        };

        var result = await controller.EditRule(ruleId, editModel);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }

    [Fact]
    public async Task EditRule_NonExistentRule_ReturnsOkWithFalse()
    {
        var editModel = new EditRuleApiModel
        {
            Id = Guid.NewGuid(),
            BehaviourId = new AndRuleBehaviour(default).GetId(),
            Parameters = "{}"
        };

        var result = await controller.EditRule(Guid.NewGuid(), editModel);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.False((bool)okResult.Value!);
    }

    [Fact]
    public async Task SetConnection_ValidConnection_ReturnsOk()
    {
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

        var outputId = Guid.NewGuid();
        var inputId = Guid.NewGuid();

        await localDbContext.Rules.AddAsync(new Rule
        {
            Id = Guid.NewGuid(),
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}",
            OutputConnectors = [new OutputPoint { Id = outputId }]
        });
        await localDbContext.Rules.AddAsync(new Rule
        {
            Id = Guid.NewGuid(),
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}",
            InputConnectors = [new InputPoint { Id = inputId }]
        });
        await localDbContext.SaveChangesAsync();

        var localRuleService = new RuleService(automatizationCacheLocal, mapper, localDbContext, connectorService, new FakeConfig(), localServiceProvider);
        var localController = new RuleController(localRuleService);

        var result = await localController.SetConnection(outputId, inputId);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SetConnection_InvalidConnection_ReturnsBadRequest()
    {
        var result = await controller.SetConnection(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task AddCustomInput_ValidInput_ReturnsOk()
    {
        var fakeConfig = new FakeConfig
        {
            Device = new ApiDevice { Id = Guid.NewGuid(), Name = "Test Device" }
        };

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

        var localRuleService = new RuleService(automatizationCacheLocal, mapper, localDbContext, connectorService, fakeConfig, localServiceProvider);
        var localController = new RuleController(localRuleService);

        var actuatorModel = new AddActuatorApiModel("TestActuator");
        var result = await localController.AddCustomInput(actuatorModel);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task AddCustomInputNode_ValidInput_ReturnsOkWithGuid()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Rule
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}"
        });
        await dbContext.SaveChangesAsync();

        var inputModel = new AddInputApiModel
        {
            RuleId = ruleId,
            InputType = InputTypeEnum.Trigger,
            Index = 0
        };

        var result = await controller.AddCustomInputNode(inputModel);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Guid>(okResult.Value);
        Assert.NotEqual(Guid.Empty, (Guid)okResult.Value!);
    }

    [Fact]
    public async Task AddCustomOutputNode_ValidOutput_ReturnsOkWithGuid()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Rule
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}"
        });
        await dbContext.SaveChangesAsync();

        var outputModel = new AddOutputApiModel
        {
            RuleId = ruleId,
            Index = 0
        };

        var result = await controller.AddCustomOutputNode(outputModel);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Guid>(okResult.Value);
        Assert.NotEqual(Guid.Empty, (Guid)okResult.Value!);
    }

    [Fact]
    public async Task RemoveCustomInputNode_ExistingRule_ReturnsOkWithTrue()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Rule
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}"
        });
        await dbContext.SaveChangesAsync();

        var result = await controller.RemoveCustomInputNode(ruleId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);
    }

    [Fact]
    public async Task RemoveCustomInputNode_NonExistentRule_ReturnsOkWithFalse()
    {
        var result = await controller.RemoveCustomInputNode(Guid.NewGuid());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.False((bool)okResult.Value!);
    }

    [Fact]
    public async Task GetRule_ExistingRule_ReturnsOkWithRule()
    {
        var ruleId = Guid.NewGuid();
        await dbContext.Rules.AddAsync(new Rule
        {
            Id = ruleId,
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}"
        });
        await dbContext.SaveChangesAsync();

        var result = await controller.GetRule(ruleId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var rule = Assert.IsType<GetRuleApiModel>(okResult.Value);
        Assert.Equal(ruleId, rule.Id);
    }

    [Fact]
    public async Task GetRule_NonExistentRule_ReturnsOkWithNull()
    {
        var result = await controller.GetRule(Guid.NewGuid());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(okResult.Value);
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
