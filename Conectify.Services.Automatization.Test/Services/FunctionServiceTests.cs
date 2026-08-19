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

public class FunctionServiceTests
{
    private readonly AutomatizationDb dbContext;
    private readonly FunctionService functionService;
    private readonly RuleService ruleService;

    public FunctionServiceTests()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        dbContext = new AutomatizationDb(contextOptions);

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventProfile>();
            cfg.AddProfile<RuleProfile>();
            cfg.AddProfile<ActuatorProfile>();
        }).CreateMapper();

        var connectorService = new ConnectorService(A.Fake<ILogger<ConnectorService>>(), new FakeConfig(), mapper, A.Fake<IHttpFactory>());

        var services = new ServiceCollection();
        services.AddTransient<IConnectorService>(_ => connectorService);
        services.AddScoped(_ => new AutomatizationDb(contextOptions));
        // Port rules read the value that feeds them while they initialize, which goes through the
        // cache the same way as in the running service.
        services.AddSingleton<IAutomatizationCache>(sp => new AutomatizationCache(sp, mapper, false));
        var serviceProvider = services.BuildServiceProvider();

        var cache = serviceProvider.GetRequiredService<IAutomatizationCache>();
        ruleService = new RuleService(cache, mapper, dbContext, connectorService, new FakeConfig(), serviceProvider, A.Fake<ILogger<RuleService>>());
        functionService = new FunctionService(cache, mapper, dbContext, ruleService, serviceProvider, A.Fake<ILogger<FunctionService>>());
    }

    [Fact]
    public async Task AddFunction_StoresItOnTheTopLevel()
    {
        var result = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Heating", result.Name);
        Assert.Null(result.ParentFunctionId);
        Assert.NotNull(dbContext.Functions.FirstOrDefault(x => x.Id == result.Id));
    }

    [Fact]
    public async Task AddFunction_InsideAnotherFunction()
    {
        var parent = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });

        var child = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Night mode", ParentFunctionId = parent.Id });

        Assert.Equal(parent.Id, child.ParentFunctionId);
    }

    [Fact]
    public async Task AddFunction_UnknownParent_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            functionService.AddFunction(new CreateFunctionApiModel { Name = "Orphan", ParentFunctionId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task AddPort_CreatesARuleWithTheRequestedSignal()
    {
        var function = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });

        var port = await functionService.AddPort(function.Id, new AddFunctionPortApiModel
        {
            Direction = FunctionPortDirection.Input,
            Type = InputTypeEnum.Trigger,
            Name = "Start",
        });

        var rule = dbContext.Rules.Include(x => x.InputConnectors).Include(x => x.OutputConnectors).First(x => x.Id == port.Id);

        Assert.Equal(function.Id, rule.FunctionId);
        Assert.Equal(FunctionPortBehaviourIds.Input, rule.RuleType);
        Assert.Equal(InputTypeEnum.Trigger, Assert.Single(rule.InputConnectors).Type);
        Assert.Single(rule.OutputConnectors);
        Assert.Equal("Start", rule.Name);
    }

    [Fact]
    public async Task AddPort_UnknownFunction_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            functionService.AddPort(Guid.NewGuid(), new AddFunctionPortApiModel { Name = "Start" }));
    }

    [Fact]
    public async Task GetAllFunctions_ReportsThePinsTheParentBoardConnectsTo()
    {
        var function = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });
        var input = await functionService.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Input, Type = InputTypeEnum.Value, Name = "Temperature" });
        var output = await functionService.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Output, Type = InputTypeEnum.Value, Name = "Command" });

        var loaded = Assert.Single(await functionService.GetAllFunctions());

        var inputPort = Assert.Single(loaded.InputPorts);
        var outputPort = Assert.Single(loaded.OutputPorts);

        // An input port is plugged into from outside, an output port is read from outside.
        Assert.Equal(dbContext.InputConnectors.First(x => x.RuleId == input.Id).Id, inputPort.PointId);
        Assert.Equal(dbContext.OutputConnectors.First(x => x.RuleId == output.Id).Id, outputPort.PointId);
        Assert.Equal("Temperature", inputPort.Name);
        Assert.Equal("Command", outputPort.Name);
    }

    [Fact]
    public async Task AddPort_StacksPortsSoTheyDoNotOverlap()
    {
        var function = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });

        var first = await functionService.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Input, Name = "A" });
        var second = await functionService.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Input, Name = "B" });

        Assert.NotEqual(dbContext.Rules.First(x => x.Id == first.Id).Y, dbContext.Rules.First(x => x.Id == second.Id).Y);

        var loaded = Assert.Single(await functionService.GetAllFunctions());
        Assert.Equal(["A", "B"], loaded.InputPorts.Select(x => x.Name));
    }

    [Fact]
    public async Task MoveRule_MovesARuleIntoAFunctionAndBackOut()
    {
        var function = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });
        var rule = await ruleService.AddNewRule(new AndRuleBehaviour(default!).GetId(), null, default);

        Assert.True(await functionService.MoveRule(rule.Id, function.Id));
        Assert.Equal(function.Id, dbContext.Rules.First(x => x.Id == rule.Id).FunctionId);

        Assert.True(await functionService.MoveRule(rule.Id, null));
        Assert.Null(dbContext.Rules.First(x => x.Id == rule.Id).FunctionId);
    }

    [Fact]
    public async Task AddNewRule_IntoAnUnknownFunction_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ruleService.AddNewRule(new AndRuleBehaviour(default!).GetId(), Guid.NewGuid(), default));
    }

    [Fact]
    public async Task AddNewRule_IntoAFunction_LandsThere()
    {
        var function = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });

        var rule = await ruleService.AddNewRule(new AndRuleBehaviour(default!).GetId(), function.Id, default);

        Assert.Equal(function.Id, rule.FunctionId);
        Assert.Equal(function.Id, dbContext.Rules.First(x => x.Id == rule.Id).FunctionId);
    }

    [Fact]
    public async Task MoveRule_UnknownFunction_ReturnsFalse()
    {
        var rule = await ruleService.AddNewRule(new AndRuleBehaviour(default!).GetId(), null, default);

        Assert.False(await functionService.MoveRule(rule.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task MoveFunction_IntoItsOwnChild_IsRefused()
    {
        var parent = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });
        var child = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Night mode", ParentFunctionId = parent.Id });

        Assert.False(await functionService.MoveFunction(parent.Id, child.Id));
        Assert.Null(dbContext.Functions.First(x => x.Id == parent.Id).ParentFunctionId);
    }

    [Fact]
    public async Task RemoveFunction_TakesNestedFunctionsAndTheirRulesWithIt()
    {
        var parent = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });
        var child = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Night mode", ParentFunctionId = parent.Id });
        var port = await functionService.AddPort(parent.Id, new AddFunctionPortApiModel { Name = "Temperature" });
        var innerRule = await ruleService.AddNewRule(new AndRuleBehaviour(default!).GetId(), child.Id, default);
        var outerRule = await ruleService.AddNewRule(new AndRuleBehaviour(default!).GetId(), null, default);

        Assert.True(await functionService.RemoveFunction(parent.Id));

        Assert.Empty(dbContext.Functions);
        Assert.Null(dbContext.Rules.FirstOrDefault(x => x.Id == port.Id));
        Assert.Null(dbContext.Rules.FirstOrDefault(x => x.Id == innerRule.Id));
        Assert.Empty(dbContext.InputConnectors.Where(x => x.RuleId == port.Id));
        // A rule outside the function is none of its business.
        Assert.NotNull(dbContext.Rules.FirstOrDefault(x => x.Id == outerRule.Id));
    }

    [Fact]
    public async Task RemoveFunction_UnknownFunction_ReturnsFalse()
    {
        Assert.False(await functionService.RemoveFunction(Guid.NewGuid()));
    }

    [Fact]
    public async Task EditFunction_RenamesAndMovesTheBox()
    {
        var function = await functionService.AddFunction(new CreateFunctionApiModel { Name = "Heating" });

        Assert.True(await functionService.EditFunction(function.Id, new EditFunctionApiModel { Name = "Heating upstairs", Description = "Bedrooms", X = 120, Y = 340 }));

        var stored = dbContext.Functions.First(x => x.Id == function.Id);
        Assert.Equal("Heating upstairs", stored.Name);
        Assert.Equal("Bedrooms", stored.Description);
        Assert.Equal(120, stored.X);
        Assert.Equal(340, stored.Y);
    }

    private class FakeConfig : ConfigurationBase, IDeviceData
    {
        public FakeConfig() : base(A.Fake<IConfiguration>())
        {
        }

        public ApiDevice Device { get; set; } = new() { Id = Guid.NewGuid(), Name = "Test device" };
        public IEnumerable<ApiSensor> Sensors { get; set; } = [];
        public IEnumerable<ApiActuator> Actuators { get; set; } = [];
        public IEnumerable<ApiPreference> Preferences { get; set; } = [];
        public IEnumerable<MetadataServiceConnector> MetadataConnectors { get; set; } = [];
    }
}
