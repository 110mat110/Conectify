using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Services;

public interface ITimingService
{
    void CreateTimingFunction(RuleDTO rule);
    void HandleTimers();
}

public class TimingService : ITimingService
{
    private readonly AutomatizationCache automatizationCache;
    private readonly IServiceProvider serviceProvider;
    private readonly Dictionary<Guid, Timer> timers = new();

    public TimingService(AutomatizationCache automatizationCache, IServiceProvider serviceProvider)
    {
        this.automatizationCache = automatizationCache;
        this.serviceProvider = serviceProvider;
    }

    public void HandleTimers()
    {
        foreach (var timer in timers)
        {
            timer.Value.Dispose();
        }

        timers.Clear();

        var setTimeRules = automatizationCache.GetRulesByTypeId(new TimeRuleBehaviour().GetId());

        foreach (var rule in setTimeRules)
        {
            CreateTimingFunction(rule);
        }
    }

    public void CreateTimingFunction(RuleDTO rule)
    {
        var parameters = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { TimeSet = "", Days = "" });
        if (parameters is null)
        {
            return;
        }

        var timer = new Timer(ExecuteTimingFunction, rule.Id, CalculateNextExecutionOfRule(parameters.TimeSet, parameters.Days), Timeout.Infinite);

        if (timers.ContainsKey(rule.Id))
        {
            timers[rule.Id].Dispose();
            timers.Remove(rule.Id);
        }

        timers.Add(rule.Id, timer);
    }

    private async void ExecuteTimingFunction(object? state)
    {
        if (state is not Guid) return;

        await timers[(Guid)state].DisposeAsync();
        timers.Remove((Guid)state);

        var rule = await automatizationCache.GetRuleByIdAsync((Guid)state);

        if (rule is null) return;

        using var scope = serviceProvider.CreateAsyncScope();
        var automatizationService = scope.ServiceProvider.GetRequiredService<IAutomatizationService>();
        await automatizationService.ExecuteRule(rule);

        CreateTimingFunction(rule);
    }

    private static long CalculateNextExecutionOfRule(string targetTimeString, string targetDaysOfWeekAbbreviations)
    {
        // Parse the target time string

        if (!DateTime.TryParse(targetTimeString, out DateTime targetTime))
        {
            throw new ArgumentException("Invalid target time format");
        }
        targetTime = targetTime.ToUniversalTime();

        // Parse the target day abbreviations
        var targetDaysOfWeek = targetDaysOfWeekAbbreviations.Split(',').Select(abbrev => ParseDayAbbreviation(abbrev)).ToList();

        // Get the current date and time
        DateTime currentTime = DateTime.UtcNow;

        // Calculate the nearest occurrence of the target day of the week
        DateTime nearestOccurrence = CalculateNearestDay(currentTime, targetDaysOfWeek);

        // Set the target date and time
        DateTime targetDateTime = new(nearestOccurrence.Year, nearestOccurrence.Month, nearestOccurrence.Day, targetTime.Hour, targetTime.Minute, targetTime.Second);

        if (targetDateTime < currentTime)
        {
            // If the target time has already passed for today, calculate for the next occurrence on an active day
            nearestOccurrence = CalculateNearestDay(nearestOccurrence.AddDays(1), targetDaysOfWeek);
            targetDateTime = new DateTime(nearestOccurrence.Year, nearestOccurrence.Month, nearestOccurrence.Day, targetTime.Hour, targetTime.Minute, targetTime.Second);
        }

        // Calculate the time difference in milliseconds
        long millisecondsUntilTarget = (long)(targetDateTime - currentTime).TotalMilliseconds;

        return millisecondsUntilTarget;
    }

    private static DayOfWeek ParseDayAbbreviation(string abbreviation)
    {
        return abbreviation.Trim().ToLower() switch
        {
            "mo" => DayOfWeek.Monday,
            "tu" => DayOfWeek.Tuesday,
            "we" => DayOfWeek.Wednesday,
            "th" => DayOfWeek.Thursday,
            "fr" => DayOfWeek.Friday,
            "sa" => DayOfWeek.Saturday,
            "su" => DayOfWeek.Sunday,
            _ => throw new ArgumentException($"Invalid day abbreviation: {abbreviation}"),
        };
    }

    private static DateTime CalculateNearestDay(DateTime currentTime, List<DayOfWeek> targetDaysOfWeek)
    {
        DateTime nearestOccurrence = DateTime.MaxValue;
        bool isNextDay = false;

        foreach (var targetDay in targetDaysOfWeek)
        {
            int daysUntilTargetDay = ((int)targetDay - (int)currentTime.DayOfWeek + 7) % 7;
            DateTime nextTargetDay = currentTime.AddDays(daysUntilTargetDay);

            if (nextTargetDay < nearestOccurrence && nextTargetDay >= currentTime)
            {
                nearestOccurrence = nextTargetDay;
                isNextDay = true;
            }
        }

        if (!isNextDay)
        {
            nearestOccurrence = CalculateNearestDay(nearestOccurrence.AddDays(1), targetDaysOfWeek);
        }

        return nearestOccurrence;
    }
}
