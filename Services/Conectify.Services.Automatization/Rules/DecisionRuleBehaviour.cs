using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Services;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class DecisionRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public MinMaxDef Outputs => new MinMaxDef(1, 1, 10);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => new List<Tuple<InputTypeEnum, MinMaxDef>>() {
            new(InputTypeEnum.Value, new(1,1,10)),
            new(InputTypeEnum.Trigger, new(0,1,1)),
            new(InputTypeEnum.Parameter, new(2,2,2))
        };
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
        var logger = serviceProvider.GetRequiredService<ILogger<DecisionRuleBehaviour>>();
        if (parameters is not null)
        {
            var pinputs = masterRule.Inputs.Where(x => x.Type == InputTypeEnum.Parameter).OrderBy(x => x.Index).ToList();
            var inputs = masterRule.Inputs.Where(x => x.Type == InputTypeEnum.Value).OrderBy(x => x.Index).ToList();
            var outputs = masterRule.Outputs.OrderBy(x => x.Index).ToList();
            if(pinputs.Count != 2 || pinputs.Any(x => x.GetEvent(serviceProvider).Result is null))
            {
                logger.LogWarning($"Not sufficient inputs. Count {pinputs.Count} P1: {pinputs.FirstOrDefault()?.GetEvent(serviceProvider)?.Result}, P2: {pinputs.Skip(1).FirstOrDefault()?.GetEvent(serviceProvider)?.Result}");
                return;
            }

            if (ComputeValue(parameters.Rule, pinputs[0].GetEvent(serviceProvider).Result!.NumericValue, pinputs[1].GetEvent(serviceProvider).Result!.NumericValue))
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    var inputEvent = await inputs[i].GetEvent(serviceProvider);
                    if (inputEvent is null)
                    {
                        continue;
                    }

                    var output = new AutomatisationEvent()
                    {
                        Name = "Comparsion output",
                        NumericValue = inputEvent!.NumericValue,
                        StringValue = inputEvent!.StringValue,
                        Unit = inputEvent!.Unit,
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        SourceId = masterRule.Id
                    };

                    if (outputs.Count > i)
                    {
                        await outputs[i].SetOutputEvent(output, ct: ct);
                    }
                }
            }
        }
    }

    Task IRuleBehaviour.InitializationValue(RuleDTO rule, RuleDTO? ruleDTO)
    {
        return Task.CompletedTask;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
    }

    public async Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var parameters = JsonConvert.DeserializeObject<DecisionOptions>(rule.ParametersJson);
        if (parameters is null)
            return;

        var cache = serviceProvider.GetRequiredService<IAutomatizationCache>();
        var ruleDTO = await cache.GetRuleByIdAsync(rule.Id);

        if (ruleDTO is null)
        {
            return;
        }

        var ps = ruleDTO.Inputs.Where(x => x.Type == InputTypeEnum.Parameter).OrderBy(x => x.Index).ToList();


        if (ps.Count != 2)
        {
            rule.Description = "You need EXACTLY 2 Parameters";
            return;
        }

        rule.Description = $"If P{ps[0].Index.ToString()} {parameters.Rule} P{ps[1].Index.ToString()} THEN I -> O";
    }

    private record DecisionOptions
    {
        public string Rule = string.Empty;
    }
}
