using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class SetDelayBehaviourTests
{
    [Fact]
    public async Task Execute_ShouldSetNextTrigger()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new RuleDTO
        {
            ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5), NextTrigger = (DateTime?)null })
        };

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        var options = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { Delay = TimeSpan.Zero, NextTrigger = (DateTime?)null });
        Assert.NotNull(options);
        Assert.NotNull(options!.NextTrigger);
    }

    [Fact]
    public async Task Execute_ShouldAskTheSchedulerToReplanTheRule()
    {
        var scheduler = A.Fake<IRuleScheduler>();
        var behaviour = new SetDelayBehaviour(new ServiceCollection().AddSingleton(scheduler).BuildServiceProvider());
        var rule = new RuleDTO
        {
            ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5) })
        };
        rule.RuleBehaviour = behaviour;

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        A.CallTo(() => scheduler.Reschedule(rule)).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WithoutDelay_ShouldFallBackToTheMinimalDelay()
    {
        // A zero delay wired back into the rule itself would otherwise spin the scheduler.
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new RuleDTO { ParametersJson = "{}" };
        var before = DateTime.UtcNow;

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        var next = behaviour.GetNextOccurrence(rule, before, null);
        Assert.NotNull(next);
        Assert.True(next >= before.AddSeconds(1), $"Expected at least a one second delay, got {next}");
    }

    [Fact]
    public void GetNextOccurrence_WithoutArmedTrigger_ShouldReturnNull()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new RuleDTO { ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5) }) };

        Assert.Null(behaviour.GetNextOccurrence(rule, DateTimeOffset.UtcNow, null));
    }

    [Fact]
    public void GetNextOccurrence_ShouldReturnTheArmedInstant()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var nextTrigger = new DateTime(2024, 5, 15, 14, 30, 0, DateTimeKind.Utc);
        var rule = new RuleDTO { ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5), NextTrigger = nextTrigger }) };

        var next = behaviour.GetNextOccurrence(rule, DateTimeOffset.UtcNow, null);

        Assert.Equal(new DateTimeOffset(nextTrigger), next);
    }

    [Fact]
    public void GetNextOccurrence_WithTriggerInThePast_ShouldStillReturnIt()
    {
        // The delay came due while the scheduler was busy - it has to run late, not be dropped.
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var nextTrigger = DateTime.UtcNow.AddHours(-2);
        var rule = new RuleDTO { ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5), NextTrigger = nextTrigger }) };

        var next = behaviour.GetNextOccurrence(rule, DateTimeOffset.UtcNow, null);

        Assert.NotNull(next);
        Assert.True(next < DateTimeOffset.UtcNow);
    }

    [Fact]
    public void GetNextOccurrence_WithTriggerAlreadyExecuted_ShouldReturnNull()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var nextTrigger = new DateTime(2024, 5, 15, 14, 30, 0, DateTimeKind.Utc);
        var rule = new RuleDTO { ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5), NextTrigger = nextTrigger }) };

        var next = behaviour.GetNextOccurrence(rule, DateTimeOffset.UtcNow, new DateTimeOffset(nextTrigger));

        Assert.Null(next);
    }

    [Fact]
    public async Task OnScheduledAsync_ShouldDisarmTheTriggerAndFireTheOutput()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var occurrence = new DateTimeOffset(2024, 5, 15, 14, 30, 0, TimeSpan.Zero);
        var output = new OutputPointDTO(Guid.NewGuid(), CreateProviderWithCache());
        var rule = new RuleDTO
        {
            ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5), NextTrigger = occurrence.UtcDateTime }),
            Outputs = [output]
        };

        await behaviour.OnScheduledAsync(rule, occurrence);

        Assert.Null(behaviour.GetNextOccurrence(rule, DateTimeOffset.UtcNow, null));
        Assert.NotNull(output.Event);
        Assert.Equal("DelayRuleResult", output.Event!.Name);
        Assert.Equal(occurrence.ToUnixTimeMilliseconds(), output.Event.TimeCreated);
    }

    [Fact]
    public async Task SetParameters_WithDelay_ShouldSetDescription()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromMinutes(1) })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Delay", rule.Description);
    }

    private static IServiceProvider CreateProviderWithCache()
    {
        return new ServiceCollection()
            .AddSingleton(A.Fake<IAutomatizationCache>())
            .AddSingleton(A.Fake<IMeterFactory>())
            .BuildServiceProvider();
    }
}
