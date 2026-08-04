using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

#pragma warning disable CS9113 // Parameter is unread. Required for Behaviour factory
public class RunAtRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehavior, ISchedulableRuleBehavior
#pragma warning restore CS9113 // Parameter is unread.
{
    /// <summary>Indexed by <see cref="DayOfWeek"/>, matches the two letter codes the UI stores.</summary>
    private static readonly string[] DayCodes = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];

    /// <summary>A weekly plan always has a hit within the next eight days (today included).</summary>
    private const int SearchWindowInDays = 8;

    public MinMaxDef Outputs => new(1, 1, 1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0,0,0)),
            new(InputTypeEnum.Trigger, new(0,0,0)),
            new(InputTypeEnum.Parameter, new(0,0,0))
        ];

    public string DisplayName() => "RUN AT";
    public Guid GetId()
    {
        return Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a");
    }

    private class TimeRuleOptions
    {
        public DateTime TimeSet { get; set; }

        public string Days { get; set; } = string.Empty;
    }

    public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// First selected weekday at the configured time of day that lies strictly after the occurrence
    /// that already ran (or after <paramref name="now"/> for a rule that has never run).
    /// </summary>
    public DateTimeOffset? GetNextOccurrence(RuleDTO rule, DateTimeOffset now, DateTimeOffset? lastOccurrence)
    {
        var options = ReadOptions(rule.ParametersJson);

        if (options is null || string.IsNullOrWhiteSpace(options.Days))
        {
            return null;
        }

        var after = lastOccurrence ?? now;
        var zone = TimeZoneInfo.Local;

        // The stored time of day is compared against local time, exactly like the previous
        // DateTime.Now based check did.
        var timeOfDay = options.TimeSet.TimeOfDay;
        var firstDay = TimeZoneInfo.ConvertTime(after, zone).Date;

        for (var offset = 0; offset < SearchWindowInDays; offset++)
        {
            var day = firstDay.AddDays(offset);

            if (!IsSelectedDay(options, day.DayOfWeek))
            {
                continue;
            }

            var localCandidate = day.Add(timeOfDay);
            var candidate = new DateTimeOffset(localCandidate, zone.GetUtcOffset(localCandidate));

            if (candidate > after)
            {
                return candidate;
            }
        }

        return null;
    }

    public Task OnScheduledAsync(RuleDTO rule, DateTimeOffset occurrence, CancellationToken ct = default)
    {
        return rule.SetAllOutputs(new AutomatisationEvent()
        {
            Name = "TimeRuleResult",
            NumericValue = 0,
            Unit = "",
            TimeCreated = occurrence.ToUnixTimeMilliseconds(),
            SourceId = rule.Id,
            Type = "Trigger",
        });
    }

    private static bool IsSelectedDay(TimeRuleOptions options, DayOfWeek dayOfWeek)
    {
        return options.Days.Contains(DayCodes[(int)dayOfWeek], StringComparison.Ordinal);
    }

    private static TimeRuleOptions? ReadOptions(string parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<TimeRuleOptions>(parametersJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    Task IRuleBehavior.InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        return Task.CompletedTask;
    }

    public Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        try
        {
            var options = JsonConvert.DeserializeObject<TimeRuleOptions>(rule.ParametersJson);

            rule.Name = $"Run at {options?.Days} {options?.TimeSet.ToLocalTime().ToShortTimeString()}";
            rule.Description = $"Run at {options?.Days} {options?.TimeSet.ToLocalTime().ToShortTimeString()}";

            return Task.CompletedTask;
        }
        catch (Exception)
        {
            rule.Name = $"Run at ";
            rule.Description = $"Run at rule, options are broken, please check options";

            return Task.CompletedTask;
        }
    }
}
