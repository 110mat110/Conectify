using Conectify.Services.Automatization.Models.DTO;

namespace Conectify.Services.Automatization.Rules;

/// <summary>
/// Implemented by behaviours that fire on their own schedule instead of reacting to an incoming event.
/// The scheduler asks for the next occurrence, sleeps exactly until it is due and then runs
/// <see cref="OnScheduledAsync"/> once - there is no polling and no failsafe re-run.
/// </summary>
public interface ISchedulableRuleBehavior
{
    /// <summary>
    /// The next instant at which the rule has to fire, or <c>null</c> when nothing is planned.
    /// A value in the past is allowed and means "should have fired already" - the scheduler runs it
    /// immediately instead of dropping it.
    /// </summary>
    /// <param name="rule">Rule the schedule is calculated for.</param>
    /// <param name="now">Current time. Used when the rule has not fired yet and needs a starting point.</param>
    /// <param name="lastOccurrence">
    /// The occurrence that was already executed, or <c>null</c> when the rule has never fired.
    /// Implementations must never return a value lower than or equal to it - that is what keeps every
    /// occurrence running exactly once.
    /// </param>
    DateTimeOffset? GetNextOccurrence(RuleDTO rule, DateTimeOffset now, DateTimeOffset? lastOccurrence);

    /// <summary>
    /// Runs a single occurrence. Called exactly once per value returned by
    /// <see cref="GetNextOccurrence"/>, even when the scheduler is late.
    /// </summary>
    Task OnScheduledAsync(RuleDTO rule, DateTimeOffset occurrence, CancellationToken ct = default);
}
