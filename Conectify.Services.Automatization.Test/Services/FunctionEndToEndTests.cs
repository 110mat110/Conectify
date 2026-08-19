using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Mapper;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Library.Services;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using Xunit.Abstractions;

namespace Conectify.Services.Automatization.Test.Services;

/// <summary>
/// Builds a whole automatization the way the UI does it and runs an event through it. The rules that
/// do the work sit inside a function, so this is what proves that a signal really crosses the border
/// of a function in both directions.
/// </summary>
public class FunctionEndToEndTests(ITestOutputHelper output)
{
    [Fact]
    public async Task EventTravelsThroughAFunctionAndComesOutAsACommand()
    {
        var world = new World();
        var actuatorId = Guid.NewGuid();
        A.CallTo(() => world.Connector.LoadActuator(actuatorId, A<CancellationToken>._))
            .Returns(Task.FromResult<ApiActuator?>(new ApiActuator { Id = actuatorId, Name = "Heater" }));

        // ---------------------------------------------------------------- the function and its pins
        var function = await world.Functions.AddFunction(new CreateFunctionApiModel { Name = "Night mode" });
        var portTemperature = await world.Functions.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Input, Type = InputTypeEnum.Value, Name = "Temperature" });
        var portStart = await world.Functions.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Input, Type = InputTypeEnum.Trigger, Name = "Start" });
        var portCommand = await world.Functions.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Output, Type = InputTypeEnum.Trigger, Name = "Command" });

        output.WriteLine($"Function '{function.Name}' with pins: Temperature (value), Start (trigger) in, Command out");

        // ---------------------------------------------------------------- what is inside the function
        var decision = await world.Rules.AddNewRule(new DecisionRuleBehaviour(world.Services).GetId(), function.Id, default);
        await world.Rules.EditRule(decision.Id, new EditRuleApiModel { Parameters = JsonConvert.SerializeObject(new { Rule = ">" }) });

        var limit = await world.AddSetValue(function.Id, 21, "limit 21 C");
        var command = await world.AddSetValue(function.Id, 1, "command 1");

        // ---------------------------------------------------------------- what is on the board outside
        var sensor = await world.Rules.AddNewRule(new InputRuleBehaviour(world.Services).GetId(), null, default);
        var heater = await world.Rules.AddNewRule(new OutputRuleBehaviour(world.Services).GetId(), null, default);
        await world.Rules.EditRule(heater.Id, new EditRuleApiModel { Parameters = JsonConvert.SerializeObject(new { DestinationId = actuatorId }) });

        output.WriteLine($"Board: [{world.Describe(sensor.Id)}] -> [function] -> [{world.Describe(heater.Id)}]");

        // ---------------------------------------------------------------- wiring, exactly as clicking pins does
        // The value pin is refreshed first, the trigger pin then starts the run inside the function.
        await world.Connect(sensor, 0, portTemperature, InputTypeEnum.Value);
        await world.Connect(sensor, 0, portStart, InputTypeEnum.Trigger);
        await world.Connect(portTemperature, 0, decision, InputTypeEnum.Parameter, 0);
        await world.Connect(limit, 0, decision, InputTypeEnum.Parameter, 1);
        await world.Connect(command, 0, decision, InputTypeEnum.Value);
        await world.Connect(portStart, 0, decision, InputTypeEnum.Trigger);
        await world.Connect(decision, 0, portCommand, InputTypeEnum.Trigger);
        await world.Connect(portCommand, 0, heater, InputTypeEnum.Trigger);

        output.WriteLine("Wired: sensor -> Temperature + Start, inside: Temperature -> P2, limit -> P3, command -> V0, Start -> T1, decision -> Command, Command -> heater");

        // ---------------------------------------------------------------- 22 degrees, above the limit
        await world.SendSensorValue(sensor.Id, 22);

        var sent = world.SentCommands();
        output.WriteLine($"Sensor reported 22 C -> commands sent: {sent.Count}, value: {sent.FirstOrDefault()?.NumericValue}");

        var only = Assert.Single(sent);
        Assert.Equal(actuatorId, only.DestinationId);
        Assert.Equal(1, only.NumericValue);
        Assert.Equal(Constants.Events.Action, only.Type);

        // ---------------------------------------------------------------- 20 degrees, below the limit
        world.ForgetCommands();
        await world.SendSensorValue(sensor.Id, 20);

        output.WriteLine($"Sensor reported 20 C -> commands sent: {world.SentCommands().Count}");
        Assert.Empty(world.SentCommands());
    }

    [Fact]
    public async Task BoardShowsTheFunctionAsOneBoxAndItsRulesOnlyInside()
    {
        var world = new World();

        var function = await world.Functions.AddFunction(new CreateFunctionApiModel { Name = "Night mode" });
        await world.Functions.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Input, Type = InputTypeEnum.Value, Name = "Temperature" });
        await world.Functions.AddPort(function.Id, new AddFunctionPortApiModel { Direction = FunctionPortDirection.Output, Type = InputTypeEnum.Trigger, Name = "Command" });
        var inside = await world.Rules.AddNewRule(new AndRuleBehaviour(world.Services).GetId(), function.Id, default);
        var outside = await world.Rules.AddNewRule(new AndRuleBehaviour(world.Services).GetId(), null, default);

        var nested = await world.Functions.AddFunction(new CreateFunctionApiModel { Name = "Weekend", ParentFunctionId = function.Id });

        var functions = (await world.Functions.GetAllFunctions()).ToList();
        var rules = (await world.Rules.GetAllRules()).ToList();

        var topLevel = functions.Where(x => x.ParentFunctionId is null).ToList();
        var boardRules = rules.Where(x => x.FunctionId is null).ToList();
        var insideRules = rules.Where(x => x.FunctionId == function.Id).ToList();

        output.WriteLine($"Top level board: {boardRules.Count} rule(s) + {topLevel.Count} function box(es)");
        output.WriteLine($"Inside '{function.Name}': {insideRules.Count} rule(s) (2 ports + 1 rule) and function '{nested.Name}'");
        output.WriteLine($"Pins drawn on the box: in [{string.Join(", ", topLevel[0].InputPorts.Select(p => p.Name))}] out [{string.Join(", ", topLevel[0].OutputPorts.Select(p => p.Name))}]");

        Assert.Single(topLevel);
        Assert.Equal(outside.Id, Assert.Single(boardRules).Id);
        Assert.Equal(3, insideRules.Count);
        Assert.Contains(insideRules, x => x.Id == inside.Id);
        Assert.Equal("Temperature", Assert.Single(topLevel[0].InputPorts).Name);
        Assert.Equal("Command", Assert.Single(topLevel[0].OutputPorts).Name);
        Assert.Equal(function.Id, Assert.Single(functions, x => x.Id == nested.Id).ParentFunctionId);
    }

    /// <summary>Everything the automatization service needs, wired the same way as in Program.cs.</summary>
    private class World
    {
        private readonly List<WebsocketEvent> sentCommands = [];
        private readonly AutomatizationDb database;
        private readonly IAutomatizationCache cache;
        private readonly IMeterFactory meterFactory = A.Fake<IMeterFactory>();

        public IConnectorService Connector { get; } = A.Fake<IConnectorService>();

        public IServiceProvider Services { get; }

        public RuleService Rules { get; }

        public FunctionService Functions { get; }

        public World()
        {
            var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
                .UseInMemoryDatabase(databaseName: "Demo-" + Guid.NewGuid())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            database = new AutomatizationDb(contextOptions);

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<EventProfile>();
                cfg.AddProfile<RuleProfile>();
                cfg.AddProfile<ActuatorProfile>();
            }).CreateMapper();

            var websocket = A.Fake<IServicesWebsocketClient>();
            A.CallTo(() => websocket.SendMessageAsync(A<WebsocketEvent>._, A<CancellationToken>._))
                .Invokes((WebsocketEvent evnt, CancellationToken _) => sentCommands.Add(evnt));

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient(_ => Connector);
            services.AddScoped(_ => new AutomatizationDb(contextOptions));
            services.AddScoped(_ => websocket);
            services.AddScoped(_ => new AutomatizationConfiguration(A.Fake<IConfiguration>()) { SensorId = Guid.NewGuid() });
            services.AddSingleton(meterFactory);
            services.AddSingleton<IAutomatizationCache>(sp => new AutomatizationCache(sp, mapper, false));
            Services = services.BuildServiceProvider();

            cache = Services.GetRequiredService<IAutomatizationCache>();
            Rules = new RuleService(cache, mapper, database, Connector, new FakeConfig(), Services, A.Fake<ILogger<RuleService>>());
            Functions = new FunctionService(cache, mapper, database, Rules, Services, A.Fake<ILogger<FunctionService>>());
        }

        public async Task<GetRuleApiModel> AddSetValue(Guid? functionId, float value, string note)
        {
            var rule = await Rules.AddNewRule(new SetValueRuleBehaviour(Services).GetId(), functionId, default);
            await Rules.EditRule(rule.Id, new EditRuleApiModel
            {
                Parameters = JsonConvert.SerializeObject(new { NumericValue = value, StringValue = note, Unit = string.Empty }),
            });

            return rule;
        }

        /// <summary>Connects an output of one rule to an input of another, the way clicking two pins does.</summary>
        public async Task Connect(GetRuleApiModel source, int outputIndex, GetRuleApiModel target, InputTypeEnum inputType, int skip = 0)
        {
            var outputId = database.OutputConnectors.Where(x => x.RuleId == source.Id).OrderBy(x => x.Index).Skip(outputIndex).First().Id;
            var inputId = database.InputConnectors.Where(x => x.RuleId == target.Id && x.Type == inputType).OrderBy(x => x.Index).Skip(skip).First().Id;

            Assert.True(await Rules.SetConnection(outputId, inputId));
        }

        public async Task SendSensorValue(Guid sensorRuleId, float value)
        {
            var rule = await cache.GetRuleByIdAsync(sensorRuleId) ?? throw new Exception("Rule is not in the cache");

            await rule.InsertEvent(new Event
            {
                Id = Guid.NewGuid(),
                Name = "Temperature",
                NumericValue = value,
                StringValue = value.ToString(),
                Unit = "C",
                SourceId = Guid.NewGuid(),
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Type = Constants.Events.Value,
            }, default, meterFactory);
        }

        public string Describe(Guid ruleId) => database.Rules.First(x => x.Id == ruleId).Name;

        public List<WebsocketEvent> SentCommands() => [.. sentCommands];

        public void ForgetCommands() => sentCommands.Clear();

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
}
