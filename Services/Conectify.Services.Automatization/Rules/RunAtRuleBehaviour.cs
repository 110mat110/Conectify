using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Newtonsoft.Json;
using System.Globalization;

namespace Conectify.Services.Automatization.Rules;

#pragma warning disable CS9113 // Parameter is unread. Required for Behaviour factory 
public class RunAtRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehavior
#pragma warning restore CS9113 // Parameter is unread.
{
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

    public async void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
        var options = JsonConvert.DeserializeObject<TimeRuleOptions>(masterRule.ParametersJson);

        if (options is not null && IsCorrectDay(options) && DateTime.Now.TimeOfDay > options.TimeSet.TimeOfDay && DateTime.Now.TimeOfDay < options.TimeSet.Add(interval).Add(interval).TimeOfDay)
        {
            await masterRule.SetAllOutputs(new AutomatisationEvent()
            {
                Name = "TimeRuleResult",
                NumericValue = 0,
                Unit = "",
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SourceId = masterRule.Id,
                Type = "Trigger",
            });
        }

    }

    private static bool IsCorrectDay(TimeRuleOptions options)
    {
        string todayShort = DateTime.Now.DayOfWeek.ToString()[..2];

        return options.Days.Contains(todayShort);
    }

    Task IRuleBehavior.InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        return Task.CompletedTask;
    }

    public Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var options = JsonConvert.DeserializeObject<TimeRuleOptions>(rule.ParametersJson); 

        rule.Name = $"Run at {options?.Days} {options?.TimeSet.ToLocalTime().ToShortTimeString()}";
        rule.Description = $"Run at {options?.Days} {options?.TimeSet.ToLocalTime().ToShortTimeString()}";

        return Task.CompletedTask;
    }
}
