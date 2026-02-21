using AutoMapper;
using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Mapper;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Automatization.Test.Services;

public class AutomatizationCacheTests
{
    private readonly IMapper mapper;

    public AutomatizationCacheTests()
    {
        mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventProfile>();
            cfg.AddProfile<RuleProfile>();
            cfg.AddProfile<ActuatorProfile>();
        }).CreateMapper();
    }

    [Fact]
    public async Task GetRuleByIdAsync_ExistingRule_ReturnsRule()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var ruleId = Guid.NewGuid();
        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = ruleId,
                RuleType = new AndRuleBehaviour(default).GetId(),
                ParametersJson = "{}"
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var result = await cache.GetRuleByIdAsync(ruleId);

        Assert.NotNull(result);
        Assert.Equal(ruleId, result.Id);
    }

    [Fact]
    public async Task GetRuleByIdAsync_NonExistingRule_ReturnsNull()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var result = await cache.GetRuleByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddNewRule_AddsRuleToDatabase()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            RuleType = new AndRuleBehaviour(default).GetId(),
            ParametersJson = "{}"
        };

        var result = await cache.AddNewRule(rule, default);

        Assert.Equal(rule.Id, result);
        using (var context = new AutomatizationDb(contextOptions))
        {
            var dbRule = await context.Rules.FindAsync(rule.Id);
            Assert.NotNull(dbRule);
        }
    }

    [Fact]
    public async Task Reload_RemovesDeletedRule()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var ruleId = Guid.NewGuid();
        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = ruleId,
                RuleType = new AndRuleBehaviour(default).GetId(),
                ParametersJson = "{}"
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var rule = await cache.GetRuleByIdAsync(ruleId);
        Assert.NotNull(rule);

        using (var context = new AutomatizationDb(contextOptions))
        {
            var dbRule = await context.Rules.FindAsync(ruleId);
            context.Rules.Remove(dbRule!);
            await context.SaveChangesAsync();
        }

        await cache.Reload(ruleId, default);
        var deletedRule = await cache.GetRuleByIdAsync(ruleId);
        Assert.Null(deletedRule);
    }

    [Fact]
    public async Task GetRulesForSourceAsync_ReturnsMatchingRules()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var sourceSensorId = Guid.NewGuid();
        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = new InputRuleBehaviour(default).GetId(),
                ParametersJson = "{\"SourceSensorId\":\"" + sourceSensorId + "\"}"
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var rules = await cache.GetRulesForSourceAsync(sourceSensorId);

        Assert.NotEmpty(rules);
    }

    [Fact]
    public async Task GetRulesByTypeIdAsync_ReturnsMatchingRules()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var ruleTypeId = new AndRuleBehaviour(default).GetId();
        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = ruleTypeId,
                ParametersJson = "{}"
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var rules = await cache.GetRulesByTypeIdAsync(ruleTypeId);

        Assert.NotEmpty(rules);
    }

    [Fact]
    public async Task GetAllRulesAsync_ReturnsAllRules()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = new AndRuleBehaviour(default).GetId(),
                ParametersJson = "{}"
            });
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = new InputRuleBehaviour(default).GetId(),
                ParametersJson = "{}"
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var rules = await cache.GetAllRulesAsync();

        Assert.True(rules.Count() >= 2);
    }

    [Fact]
    public async Task GetOutputPointById_ExistingOutput_ReturnsOutput()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var outputId = Guid.NewGuid();
        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = new AndRuleBehaviour(default).GetId(),
                ParametersJson = "{}",
                OutputConnectors = [new OutputPoint { Id = outputId }]
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var output = await cache.GetOutputPointById(outputId);

        Assert.NotNull(output);
        Assert.Equal(outputId, output.Id);
    }

    [Fact]
    public async Task GetInputPointById_ExistingInput_ReturnsInput()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var inputId = Guid.NewGuid();
        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = new AndRuleBehaviour(default).GetId(),
                ParametersJson = "{}",
                InputConnectors = [new InputPoint { Id = inputId }]
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var input = await cache.GetInputPointById(inputId);

        Assert.NotNull(input);
        Assert.Equal(inputId, input.Id);
    }

    [Fact]
    public void ConnectionExist_ExistingConnection_ReturnsTrue()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        using (var context = new AutomatizationDb(contextOptions))
        {
            context.Set<RuleConnector>().Add(new RuleConnector
            {
                SourceRuleId = sourceId,
                TargetRuleId = targetId
            });
            context.SaveChanges();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, true);
        var exists = cache.ConnectionExist(sourceId, targetId);

        Assert.True(exists);
    }

    [Fact]
    public void ConnectionExist_NonExistingConnection_ReturnsFalse()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        var exists = cache.ConnectionExist(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(exists);
    }

    [Fact]
    public async Task ReloadConnections_LoadsAllConnections()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        using (var context = new AutomatizationDb(contextOptions))
        {
            context.Set<RuleConnector>().Add(new RuleConnector
            {
                SourceRuleId = sourceId,
                TargetRuleId = targetId
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, false);
        await cache.ReloadConnections();

        var exists = cache.ConnectionExist(sourceId, targetId);
        Assert.True(exists);
    }

    [Fact]
    public async Task GetNextInputs_ReturnsConnectedInputs()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var outputId = Guid.NewGuid();
        var inputId = Guid.NewGuid();

        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = new AndRuleBehaviour(default).GetId(),
                ParametersJson = "{}",
                InputConnectors = [new InputPoint { Id = inputId }],
                OutputConnectors = [new OutputPoint { Id = outputId }]
            });
            context.Set<RuleConnector>().Add(new RuleConnector
            {
                SourceRuleId = outputId,
                TargetRuleId = inputId
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, true);
        var inputs = await cache.GetNextInputs(outputId);

        Assert.NotEmpty(inputs);
    }

    [Fact]
    public async Task GetPreviousOutputs_ReturnsConnectedOutputs()
    {
        var contextOptions = new DbContextOptionsBuilder<AutomatizationDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(services => new AutomatizationDb(contextOptions));
        var serviceProvider = services.BuildServiceProvider();

        var outputId = Guid.NewGuid();
        var inputId = Guid.NewGuid();

        using (var context = new AutomatizationDb(contextOptions))
        {
            await context.Rules.AddAsync(new Rule
            {
                Id = Guid.NewGuid(),
                RuleType = new AndRuleBehaviour(default).GetId(),
                ParametersJson = "{}",
                InputConnectors = [new InputPoint { Id = inputId }],
                OutputConnectors = [new OutputPoint { Id = outputId }]
            });
            context.Set<RuleConnector>().Add(new RuleConnector
            {
                SourceRuleId = outputId,
                TargetRuleId = inputId
            });
            await context.SaveChangesAsync();
        }

        var cache = new AutomatizationCache(serviceProvider, mapper, true);
        var outputs = await cache.GetPreviousOutputs(inputId);

        Assert.NotEmpty(outputs);
    }
}
