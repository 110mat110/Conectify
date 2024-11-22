using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class DecisionRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public int DefaultOutputs => 1;

    public IEnumerable<Tuple<InputTypeEnum, int>> DefaultInputs => new List<Tuple<InputTypeEnum, int>>() { new Tuple<InputTypeEnum, int>(InputTypeEnum.Value, 2), new Tuple<InputTypeEnum, int>(InputTypeEnum.Trigger, 1), new Tuple<InputTypeEnum, int>(InputTypeEnum.Parameter, 1) };

    public string DisplayName() => "DECISION";

    public Guid GetId()
    {
        return Guid.Parse("62d50548-fff0-44c4-8bf3-b592042b1c2b");
    }

    private bool ComputeValue(string param, float? input, float? comparingValue)
    {
        return param switch
        {
            ">" => input > comparingValue,
            "<" => input < comparingValue,
            "=" => input == comparingValue,
            ">=" => input >= comparingValue,
            "<=" => input <= comparingValue,
            "<>" => input != comparingValue,
            "!=" => input != comparingValue,
            _ => false,
        };
    }

    public async Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        var parameters = JsonConvert.DeserializeObject<DecisionOptions>(masterRule.ParametersJson);

        if (parameters is not null)
        {
            var pinputs = masterRule.Inputs.Where(x => x.Type == InputTypeEnum.Parameter).ToList();
            var inputs = masterRule.Inputs.Where(x => x.Type == InputTypeEnum.Value).ToList();
            var outputs = masterRule.Outputs.ToList();
            if(pinputs.Count != 2 || pinputs.Any(x => x.AutomatisationValue is null) || inputs.Count != outputs.Count)
            {
                return;
            }

            if (ComputeValue(parameters.Rule, pinputs[0].AutomatisationValue!.NumericValue, pinputs[1].AutomatisationValue!.NumericValue))
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    if(inputs[i].AutomatisationValue is null)
                    {
                        continue;
                    }

                    var output = new AutomatisationEvent()
                    {
                        Name = "Comparsion output",
                        NumericValue = inputs[i].AutomatisationValue!.NumericValue,
                        StringValue = inputs[i].AutomatisationValue!.StringValue,
                        Unit = inputs[i].AutomatisationValue!.Unit,
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        SourceId = masterRule.Id
                    };

                    await outputs[i].SetOutputEvent(output);
                }
            }
        }
    }

    Task IRuleBehaviour.InitializationValue(RuleDTO rule)
    {
        return Task.CompletedTask;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
    }

    private record DecisionOptions
    {
        public string Rule = string.Empty;
    }
}
