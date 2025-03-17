using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class SetValueRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public MinMaxDef Outputs => new(1, 1, 1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0,0,0)),
            new(InputTypeEnum.Trigger, new(0,0,1)),
            new(InputTypeEnum.Parameter, new(0,0,0))
        ];
    public string DisplayName() => "SET VALUE";
    public Guid GetId()
    {
        return Guid.Parse("8c173ffc-7243-4675-9a0d-28c2ce19a18f");
    }

    public async Task Execute(RuleDTO rule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        var value = JsonConvert.DeserializeObject<SetValueOptions>(rule.ParametersJson);

        if (value is not null)
        {

            var evnt = new AutomatisationEvent()
            {
                Name = "Static value",
                NumericValue = value.NumericValue,
                StringValue = value.StringValue,
                Unit = value.Unit,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SourceId = rule.Id,
            };

            await rule.SetAllOutputs(evnt, true);
        }
        return;
    }

    public async Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        var value = JsonConvert.DeserializeObject<SetValueOptions>(rule.ParametersJson);

        if (value is not null)
        {

            var evnt = new AutomatisationEvent()
            {
                Name = "Static value",
                NumericValue = value.NumericValue,
                StringValue = value.StringValue,
                Unit = value.Unit,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SourceId = rule.Id,
            };

            await rule.SetAllOutputs(evnt, false);
        }
        return;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
    }

    public async Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var value = JsonConvert.DeserializeObject<SetValueOptions>(rule.ParametersJson);
        if (value is null) return; 
        var stringValue = !string.IsNullOrEmpty(value.StringValue) ? $" || {value.StringValue} {value.Unit}" : string.Empty;
        rule.Description = $"{value.NumericValue}{value.Unit}{stringValue} ";
    }

    private record SetValueOptions
    {
        public string StringValue = string.Empty;
        public float NumericValue = 0;
        public string Unit = string.Empty;
    }
}
