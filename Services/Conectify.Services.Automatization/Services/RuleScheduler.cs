using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Services;

/// <summary>
/// Snapshot of one planned rule, exposed for diagnostics and tests.
/// </summary>
public sealed record ScheduledRuleInfo(Guid RuleId, string RuleName, DateTimeOffset? DueAt, DateTimeOffset? LastOccurrence, bool IsRunning);

public interface IRuleScheduler
{
    /// <summary>Starts the scheduling loop. Repeated calls are ignored.</summary>
    void Start();

    /// <summary>Stops the scheduling loop and waits for it to finish.</summary>
    Task StopAsync();

    /// <summary>
    /// Recalculates the plan of a single rule, e.g. after a behaviour armed a new delay.
    /// Safe to call from a rule behaviour while the rule itself is running.
    /// </summary>
    void Reschedule(RuleDTO rule);

    /// <summary>Rebuilds the whole plan from the rule cache.</summary>
    Task SyncAsync(CancellationToken ct = default);

    IReadOnlyCollection<ScheduledRuleInfo> GetPlan();
}

/// <summary>
/// Runs time based rules. Instead of ticking every second and asking every rule whether its moment
/// has come, every schedulable rule reports the exact instant it wants to run at and the loop sleeps
/// until the closest one is due.
///
/// Guarantees:
/// <list type="bullet">
/// <item>An occurrence never runs twice - it is marked as consumed before the behaviour is invoked
/// and a behaviour can never hand out an occurrence that is not strictly newer than the last one.</item>
/// <item>An occurrence is never skipped - a due job stays due until it actually runs, so a blocked
/// thread, a slow rule or a clock jump make it run late rather than not at all.</item>
/// </list>
/// </summary>
public sealed class RuleScheduler : IRuleScheduler, IDisposable
{
    /// <summary>
    /// Upper bound for a single sleep. The wait itself is monotonic, so capping it makes the loop
    /// notice wall clock changes (NTP correction, DST, suspended machine) within this period.
    /// </summary>
    private static readonly TimeSpan MaxWait = TimeSpan.FromSeconds(30);

    /// <summary>Fallback re-read of the rule cache in case a change notification is missed.</summary>
    private static readonly TimeSpan SyncInterval = TimeSpan.FromSeconds(30);

    private readonly IAutomatizationCache cache;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<RuleScheduler> logger;

    private readonly object gate = new();
    private readonly Dictionary<Guid, ScheduledJob> jobs = [];
    private readonly SemaphoreSlim wakeSignal = new(0, 1);

    private CancellationTokenSource? cts;
    private Task? loop;
    private int syncRequested = 1;
    private DateTimeOffset lastSync = DateTimeOffset.MinValue;

    public RuleScheduler(IAutomatizationCache cache, TimeProvider timeProvider, ILogger<RuleScheduler> logger)
    {
        this.cache = cache;
        this.timeProvider = timeProvider;
        this.logger = logger;

        cache.RulesChanged += OnRulesChanged;
    }

    public void Start()
    {
        CancellationToken token;
        lock (gate)
        {
            if (loop is not null)
            {
                return;
            }

            cts = new CancellationTokenSource();
            token = cts.Token;
            loop = Task.Run(() => RunAsync(token), token);
        }

        logger.LogInformation("Rule scheduler started");
    }

    public async Task StopAsync()
    {
        Task? running;
        lock (gate)
        {
            if (cts is null)
            {
                return;
            }

            cts.Cancel();
            running = loop;
            loop = null;
        }

        if (running is not null)
        {
            try
            {
                await running;
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
        }

        logger.LogInformation("Rule scheduler stopped");
    }

    public void Reschedule(RuleDTO rule)
    {
        if (rule.RuleBehaviour is not ISchedulableRuleBehavior behaviour)
        {
            return;
        }

        lock (gate)
        {
            Upsert(rule, behaviour, timeProvider.GetUtcNow());
        }

        Wake();
    }

    public async Task SyncAsync(CancellationToken ct = default)
    {
        var rules = await cache.GetAllRulesAsync();
        var schedulable = rules.Where(x => x.RuleBehaviour is ISchedulableRuleBehavior).ToList();

        lock (gate)
        {
            var now = timeProvider.GetUtcNow();
            var alive = new HashSet<Guid>();

            foreach (var rule in schedulable)
            {
                alive.Add(rule.Id);
                Upsert(rule, (ISchedulableRuleBehavior)rule.RuleBehaviour!, now);
            }

            foreach (var id in jobs.Keys.Where(x => !alive.Contains(x)).ToList())
            {
                jobs[id].IsRemoved = true;
                jobs.Remove(id);
                logger.LogInformation("Rule {RuleId} is not schedulable anymore, dropped from the plan", id);
            }
        }

        Wake();
    }

    public IReadOnlyCollection<ScheduledRuleInfo> GetPlan()
    {
        lock (gate)
        {
            return jobs.Values
                .Select(x => new ScheduledRuleInfo(x.RuleId, x.Rule.Name, x.DueAt, x.LastOccurrence, x.IsRunning))
                .ToList();
        }
    }

    public void Dispose()
    {
        cache.RulesChanged -= OnRulesChanged;

        lock (gate)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            loop = null;
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await SyncIfNeededAsync(ct);

                var due = TakeDueJobs();
                foreach (var (job, occurrence) in due)
                {
                    _ = FireAsync(job, occurrence, ct);
                }

                // Something was dispatched - recompute straight away, the dispatched jobs are marked
                // as running so they cannot be picked up a second time.
                if (due.Count > 0)
                {
                    continue;
                }

                await WaitForNextAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Rule scheduler loop failed, retrying");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task SyncIfNeededAsync(CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        var requested = Interlocked.Exchange(ref syncRequested, 0) == 1;

        if (!requested && now - lastSync < SyncInterval)
        {
            return;
        }

        lastSync = now;

        try
        {
            await SyncAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Rule scheduler could not synchronise with the rule cache, keeping the current plan");
        }
    }

    /// <summary>
    /// Claims every job that is due. Claiming marks the job as running and hands out its occurrence
    /// under the same lock, so an occurrence can only ever be dispatched by one caller.
    /// </summary>
    private List<(ScheduledJob Job, DateTimeOffset Occurrence)> TakeDueJobs()
    {
        var due = new List<(ScheduledJob, DateTimeOffset)>();

        lock (gate)
        {
            var now = timeProvider.GetUtcNow();
            foreach (var job in jobs.Values)
            {
                if (job.IsRunning || job.DueAt is null || job.DueAt > now)
                {
                    continue;
                }

                job.IsRunning = true;
                due.Add((job, job.DueAt.Value));
            }
        }

        return due;
    }

    private async Task FireAsync(ScheduledJob job, DateTimeOffset occurrence, CancellationToken ct)
    {
        var rule = job.Rule;

        try
        {
            logger.LogInformation("Scheduler running rule {RuleId} ({RuleName}) for occurrence {Occurrence:o}",
                job.RuleId, rule.Name, occurrence);

            await job.Behaviour.OnScheduledAsync(rule, occurrence, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Rule {RuleId} failed for occurrence {Occurrence:o}", job.RuleId, occurrence);
        }
        finally
        {
            lock (gate)
            {
                // The occurrence is consumed even when the rule threw. Retrying it here would be the
                // same double run the old timer based failsafe produced.
                job.LastOccurrence = occurrence;
                job.IsRunning = false;

                if (!job.IsRemoved)
                {
                    Plan(job, timeProvider.GetUtcNow());
                }
            }

            Wake();
        }
    }

    private async Task WaitForNextAsync(CancellationToken ct)
    {
        TimeSpan wait;
        lock (gate)
        {
            wait = TimeUntilNextDue(timeProvider.GetUtcNow());
        }

        if (wait <= TimeSpan.Zero)
        {
            return;
        }

        await wakeSignal.WaitAsync(wait, ct);
    }

    private TimeSpan TimeUntilNextDue(DateTimeOffset now)
    {
        var next = DateTimeOffset.MaxValue;
        foreach (var job in jobs.Values)
        {
            if (job.IsRunning || job.DueAt is null)
            {
                continue;
            }

            if (job.DueAt.Value < next)
            {
                next = job.DueAt.Value;
            }
        }

        if (next == DateTimeOffset.MaxValue)
        {
            return MaxWait;
        }

        var delta = next - now;
        if (delta <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        return delta < MaxWait ? delta : MaxWait;
    }

    private void Upsert(RuleDTO rule, ISchedulableRuleBehavior behaviour, DateTimeOffset now)
    {
        if (!jobs.TryGetValue(rule.Id, out var job))
        {
            job = new ScheduledJob(rule.Id);
            jobs[rule.Id] = job;
        }

        // The cache replaces DTO instances on every reload, always plan against the current one.
        job.Rule = rule;
        job.Behaviour = behaviour;

        // A running job replans itself once it finishes, overwriting it now would lose that result.
        if (job.IsRunning)
        {
            return;
        }

        Plan(job, now);
    }

    private void Plan(ScheduledJob job, DateTimeOffset now)
    {
        DateTimeOffset? next;
        try
        {
            next = job.Behaviour.GetNextOccurrence(job.Rule, now, job.LastOccurrence);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot calculate the next occurrence of rule {RuleId}, it stays idle", job.RuleId);
            next = null;
        }

        // Backstop for the exactly once guarantee - a behaviour handing out an occurrence that was
        // already executed would run it a second time.
        if (next is not null && job.LastOccurrence is not null && next <= job.LastOccurrence)
        {
            logger.LogWarning("Rule {RuleId} asked for occurrence {Occurrence:o} which already ran at {LastOccurrence:o}, ignoring it",
                job.RuleId, next, job.LastOccurrence);
            next = null;
        }

        if (job.DueAt != next)
        {
            logger.LogInformation("Rule {RuleId} ({RuleName}) planned for {Occurrence}", job.RuleId, job.Rule.Name,
                next?.ToString("o") ?? "never");
        }

        job.DueAt = next;
    }

    private void OnRulesChanged()
    {
        Interlocked.Exchange(ref syncRequested, 1);
        Wake();
    }

    private void Wake()
    {
        if (wakeSignal.CurrentCount > 0)
        {
            return;
        }

        try
        {
            wakeSignal.Release();
        }
        catch (SemaphoreFullException)
        {
            // another thread got there first
        }
        catch (ObjectDisposedException)
        {
            // scheduler is being torn down
        }
    }

    private sealed class ScheduledJob(Guid ruleId)
    {
        public Guid RuleId => ruleId;

        public RuleDTO Rule { get; set; } = null!;

        public ISchedulableRuleBehavior Behaviour { get; set; } = null!;

        public DateTimeOffset? DueAt { get; set; }

        public DateTimeOffset? LastOccurrence { get; set; }

        public bool IsRunning { get; set; }

        public bool IsRemoved { get; set; }
    }
}
