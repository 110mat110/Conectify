using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class RunAtRuleBehaviourTests
{
    [Fact]
    public void GetId_ShouldReturnCorrectGuid()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        Assert.Equal(Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a"), behaviour.GetId());
    }

    [Fact]
    public void DisplayName_ShouldReturnRunAt()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        Assert.Equal("RUN AT", behaviour.DisplayName());
    }

    [Fact]
    public void Outputs_ShouldHaveCorrectConfiguration()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        var outputs = behaviour.Outputs;

        Assert.Equal(1, outputs.Min);
        Assert.Equal(1, outputs.Def);
        Assert.Equal(1, outputs.Max);
    }

    [Fact]
    public async Task Execute_ShouldCompleteSuccessfully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var ruleDTO = new RuleDTO();

        await behaviour.Execute(ruleDTO, new AutomatisationEvent());

        Assert.True(true);
    }

    [Fact]
    public async Task SetParameters_WithTimeAndDays_ShouldSetNameAndDescription()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var timeSet = new DateTime(2024, 1, 1, 14, 30, 0, DateTimeKind.Utc);
        var days = "Mo,We,Fr";
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { TimeSet = timeSet, Days = days })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Run at", rule.Name);
        Assert.Contains(days, rule.Name);
        Assert.Contains("Run at", rule.Description);
        Assert.Contains(days, rule.Description);
    }

    [Fact]
    public async Task SetParameters_WithoutDays_ShouldHandleGracefully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var timeSet = new DateTime(2024, 1, 1, 14, 30, 0, DateTimeKind.Utc);
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { TimeSet = timeSet })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Run at", rule.Name);
    }

    [Fact]
    public async Task SetParameters_WithEmptyJson_ShouldHandleGracefully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = "{}"
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.NotNull(rule.Name);
        Assert.NotNull(rule.Description);
    }

    [Fact]
    public async Task SetParameters_WithNullJson_ShouldHandleGracefully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = null
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.NotNull(rule.Name);
        Assert.NotNull(rule.Description);
    }

    [Fact]
    public async Task SetParameters_WithInvalidJson_ShouldHandleGracefully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = "invalid json"
        };

            await behaviour.SetParameters(rule, CancellationToken.None);
        Assert.NotNull(rule.Name);
        Assert.NotNull(rule.Description);
    }

    [Fact]
    public async Task SetParameters_WithDifferentTimeFormats_ShouldParse()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var timeSet = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var days = "Mo,Tu,We,Th,Fr,Sa,Su";
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { TimeSet = timeSet, Days = days })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Run at", rule.Name);
        Assert.Contains(days, rule.Name);
    }

    [Fact]
    public void GetNextOccurrence_WithoutLastRun_ShouldReturnTheNextMatchingSlot()
    {
        // Wednesday 12:00 local, rule fires on Wednesdays at 14:30.
        var now = Local(2024, 5, 15, 12, 0, 0);
        var behaviour = CreateBehaviour();
        var rule = CreateRule("14:30:00", "We");

        var next = behaviour.GetNextOccurrence(rule, now, null);

        Assert.Equal(Local(2024, 5, 15, 14, 30, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_AfterTodaysSlotPassed_ShouldJumpToTheNextSelectedDay()
    {
        // Wednesday 15:00 local, today's 14:30 is gone so the next Friday is due.
        var now = Local(2024, 5, 15, 15, 0, 0);
        var behaviour = CreateBehaviour();
        var rule = CreateRule("14:30:00", "We,Fr");

        var next = behaviour.GetNextOccurrence(rule, now, null);

        Assert.Equal(Local(2024, 5, 17, 14, 30, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_AfterARun_ShouldReturnTheFollowingWeek()
    {
        var behaviour = CreateBehaviour();
        var rule = CreateRule("14:30:00", "We");
        var lastRun = Local(2024, 5, 15, 14, 30, 0);

        var next = behaviour.GetNextOccurrence(rule, lastRun, lastRun);

        Assert.Equal(Local(2024, 5, 22, 14, 30, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_ShouldNeverReturnTheOccurrenceThatAlreadyRan()
    {
        // Every day at 14:30 - the moment it fires, the next plan has to be tomorrow.
        var behaviour = CreateBehaviour();
        var rule = CreateRule("14:30:00", "Mo,Tu,We,Th,Fr,Sa,Su");
        var lastRun = Local(2024, 5, 15, 14, 30, 0);

        var next = behaviour.GetNextOccurrence(rule, lastRun, lastRun);

        Assert.NotNull(next);
        Assert.True(next > lastRun);
        Assert.Equal(Local(2024, 5, 16, 14, 30, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_WithoutSelectedDays_ShouldReturnNull()
    {
        var behaviour = CreateBehaviour();
        var rule = CreateRule("14:30:00", string.Empty);

        Assert.Null(behaviour.GetNextOccurrence(rule, Local(2024, 5, 15, 12, 0, 0), null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("not a json")]
    public void GetNextOccurrence_WithBrokenParameters_ShouldReturnNull(string parametersJson)
    {
        var behaviour = CreateBehaviour();
        var rule = new RuleDTO { ParametersJson = parametersJson };

        Assert.Null(behaviour.GetNextOccurrence(rule, Local(2024, 5, 15, 12, 0, 0), null));
    }

    [Fact]
    public async Task OnScheduledAsync_ShouldStampTheEventWithTheOccurrence()
    {
        var behaviour = CreateBehaviour();
        var output = new OutputPointDTO(Guid.NewGuid(), CreateProviderWithCache());
        var rule = CreateRule("14:30:00", "We");
        rule.Outputs = [output];
        var occurrence = Local(2024, 5, 15, 14, 30, 0);

        await behaviour.OnScheduledAsync(rule, occurrence);

        Assert.NotNull(output.Event);
        Assert.Equal("TimeRuleResult", output.Event!.Name);
        Assert.Equal("Trigger", output.Event.Type);
        Assert.Equal(occurrence.ToUnixTimeMilliseconds(), output.Event.TimeCreated);
    }

    private static RunAtRuleBehaviour CreateBehaviour() => new(new ServiceCollection().BuildServiceProvider());

    /// <summary>Firing an output walks the rule graph, which needs the cache to find the next inputs.</summary>
    private static IServiceProvider CreateProviderWithCache()
    {
        return new ServiceCollection()
            .AddSingleton(A.Fake<IAutomatizationCache>())
            .AddSingleton(A.Fake<IMeterFactory>())
            .BuildServiceProvider();
    }

    private static RuleDTO CreateRule(string timeOfDay, string days)
    {
        return new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { TimeSet = $"2024-01-01T{timeOfDay}", Days = days })
        };
    }

    private static DateTimeOffset Local(int year, int month, int day, int hour, int minute, int second)
    {
        var local = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local));
    }

    [Fact]
    public void GetId_ShouldBeConsistentAcrossInstances()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour1 = new RunAtRuleBehaviour(serviceProvider);
        var behaviour2 = new RunAtRuleBehaviour(serviceProvider);

        Assert.Equal(behaviour1.GetId(), behaviour2.GetId());
    }
}
