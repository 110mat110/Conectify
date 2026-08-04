using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;

namespace Conectify.Services.Automatization.Test.Services;

public class RuleSchedulerTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task DueOccurrence_RunsExactlyOnce()
    {
        var behaviour = new RecordingBehaviour(planNext: false);
        var rule = CreateRule(behaviour);
        using var scheduler = CreateScheduler(rule);

        scheduler.Start();

        await WaitUntil(() => behaviour.RunCount >= 1);
        // Give the loop plenty of iterations to run it a second time if the plan was wrong.
        await Task.Delay(300);
        await scheduler.StopAsync();

        Assert.Equal(1, behaviour.RunCount);
    }

    [Fact]
    public async Task RecurringRule_NeverRepeatsTheSameOccurrence()
    {
        var behaviour = new RecordingBehaviour(planNext: true, step: TimeSpan.FromMilliseconds(20));
        var rule = CreateRule(behaviour);
        using var scheduler = CreateScheduler(rule);

        scheduler.Start();

        await WaitUntil(() => behaviour.RunCount >= 5);
        await scheduler.StopAsync();

        Assert.Equal(behaviour.Occurrences.Count, behaviour.Occurrences.Distinct().Count());
        Assert.Equal(behaviour.Occurrences.OrderBy(x => x), behaviour.Occurrences);
    }

    [Fact]
    public async Task OccurrenceInThePast_RunsImmediatelyInsteadOfBeingSkipped()
    {
        // Simulates a delay that came due while the scheduler was busy or the process was blocked.
        var behaviour = new RecordingBehaviour(planNext: false)
        {
            FirstOccurrence = DateTimeOffset.UtcNow.AddHours(-3)
        };
        var rule = CreateRule(behaviour);
        using var scheduler = CreateScheduler(rule);

        scheduler.Start();

        await WaitUntil(() => behaviour.RunCount >= 1);
        await scheduler.StopAsync();

        Assert.Single(behaviour.Occurrences);
    }

    [Fact]
    public async Task FailingRule_DoesNotRetryTheSameOccurrenceAndKeepsTheLoopAlive()
    {
        var behaviour = new RecordingBehaviour(planNext: true, step: TimeSpan.FromMilliseconds(20))
        {
            ThrowOnRun = true
        };
        var rule = CreateRule(behaviour);
        using var scheduler = CreateScheduler(rule);

        scheduler.Start();

        await WaitUntil(() => behaviour.RunCount >= 3);
        await scheduler.StopAsync();

        Assert.Equal(behaviour.Occurrences.Count, behaviour.Occurrences.Distinct().Count());
    }

    [Fact]
    public async Task AnOccurrenceAlreadyExecuted_IsRejectedWhenTheBehaviourOffersItAgain()
    {
        // A behaviour that keeps returning the very same instant must not make it run twice.
        var behaviour = new RecordingBehaviour(planNext: true, step: TimeSpan.Zero);
        var rule = CreateRule(behaviour);
        using var scheduler = CreateScheduler(rule);

        scheduler.Start();

        await WaitUntil(() => behaviour.RunCount >= 1);
        await Task.Delay(300);
        await scheduler.StopAsync();

        Assert.Equal(1, behaviour.RunCount);
    }

    [Fact]
    public async Task RuleRemovedFromCache_IsDroppedFromThePlan()
    {
        var behaviour = new RecordingBehaviour(planNext: true, step: TimeSpan.FromMilliseconds(20));
        var rule = CreateRule(behaviour);
        var rules = new List<RuleDTO> { rule };
        using var scheduler = CreateScheduler(rules);

        scheduler.Start();
        await WaitUntil(() => behaviour.RunCount >= 1);

        rules.Clear();
        await scheduler.SyncAsync();
        await scheduler.StopAsync();

        Assert.Empty(scheduler.GetPlan());
    }

    [Fact]
    public async Task NonSchedulableRule_IsNotPlanned()
    {
        var rule = new RuleDTO { Id = Guid.NewGuid(), RuleBehaviour = A.Fake<IRuleBehavior>() };
        using var scheduler = CreateScheduler(rule);

        await scheduler.SyncAsync();

        Assert.Empty(scheduler.GetPlan());
    }

    [Fact]
    public async Task Reschedule_PlansARuleThatWasIdle()
    {
        var behaviour = new RecordingBehaviour(planNext: false)
        {
            FirstOccurrence = null
        };
        var rule = CreateRule(behaviour);
        using var scheduler = CreateScheduler(rule);

        scheduler.Start();
        await scheduler.SyncAsync();

        Assert.Null(scheduler.GetPlan().Single().DueAt);

        behaviour.FirstOccurrence = DateTimeOffset.UtcNow;
        scheduler.Reschedule(rule);

        await WaitUntil(() => behaviour.RunCount >= 1);
        await scheduler.StopAsync();

        Assert.Equal(1, behaviour.RunCount);
    }

    private static RuleDTO CreateRule(RecordingBehaviour behaviour)
    {
        return new RuleDTO { Id = Guid.NewGuid(), Name = "test rule", RuleBehaviour = behaviour };
    }

    private static RuleScheduler CreateScheduler(RuleDTO rule) => CreateScheduler([rule]);

    private static RuleScheduler CreateScheduler(List<RuleDTO> rules)
    {
        var cache = A.Fake<IAutomatizationCache>();
        A.CallTo(() => cache.GetAllRulesAsync()).ReturnsLazily(() => Task.FromResult<IEnumerable<RuleDTO>>([.. rules]));

        return new RuleScheduler(cache, TimeProvider.System, A.Fake<ILogger<RuleScheduler>>());
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.Add(Timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(condition(), $"Condition was not met within {Timeout}");
    }

    /// <summary>
    /// Schedulable behaviour that records every occurrence it was asked to run.
    /// </summary>
    private sealed class RecordingBehaviour(bool planNext, TimeSpan step = default) : IRuleBehavior, ISchedulableRuleBehavior
    {
        private readonly object sync = new();
        private readonly List<DateTimeOffset> occurrences = [];

        public DateTimeOffset? FirstOccurrence { get; set; } = DateTimeOffset.UtcNow;

        public bool ThrowOnRun { get; set; }

        public int RunCount
        {
            get
            {
                lock (sync)
                {
                    return occurrences.Count;
                }
            }
        }

        public IReadOnlyList<DateTimeOffset> Occurrences
        {
            get
            {
                lock (sync)
                {
                    return [.. occurrences];
                }
            }
        }

        public DateTimeOffset? GetNextOccurrence(RuleDTO rule, DateTimeOffset now, DateTimeOffset? lastOccurrence)
        {
            if (lastOccurrence is null)
            {
                return FirstOccurrence;
            }

            return planNext ? lastOccurrence.Value.Add(step) : null;
        }

        public Task OnScheduledAsync(RuleDTO rule, DateTimeOffset occurrence, CancellationToken ct = default)
        {
            lock (sync)
            {
                occurrences.Add(occurrence);
            }

            if (ThrowOnRun)
            {
                throw new InvalidOperationException("rule blew up");
            }

            return Task.CompletedTask;
        }

        public MinMaxDef Outputs => new(1, 1, 1);

        public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [];

        public Guid GetId() => Guid.Parse("0f1f8b6a-6e2c-4a3f-8b2d-1c7f0a9e5d31");

        public string DisplayName() => "TEST";

        public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default) => Task.CompletedTask;

        public Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO) => Task.CompletedTask;

        public Task SetParameters(Rule rule, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
