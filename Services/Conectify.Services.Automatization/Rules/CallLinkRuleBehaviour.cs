using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Library;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static Conectify.Services.Automatization.Rules.OutputRuleBehaviour;

namespace Conectify.Services.Automatization.Rules;

public class CallLinkRuleBehavior(IServiceProvider serviceProvider) : IRuleBehavior
{

    public MinMaxDef Outputs => new(0, 0, 0);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(1,1,1)),
            new(InputTypeEnum.Trigger, new(0,1,1)),
            new(InputTypeEnum.Parameter, new(0,0,0))
        ];
    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
    }

    public string DisplayName() => "Call HTTP";

    public async Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        var options = JsonConvert.DeserializeObject<CallLinkRuleOptions>(masterRule.ParametersJson);
        var address = options?.Http ?? throw new Exception("Invalid URL");
        address = address.Replace("{numericValue}", (await masterRule.Inputs.FirstOrDefault(x => x.Type == InputTypeEnum.Value)?.GetEvent(serviceProvider))?.NumericValue.ToString() ?? string.Empty);
        address = address.Replace("{stringValue}",(await masterRule.Inputs.FirstOrDefault(x => x.Type == InputTypeEnum.Value)?.GetEvent(serviceProvider))?.StringValue.ToString() ?? string.Empty);


        HttpClient client = new HttpClient();
        _ =  await client.GetAsync(address, ct);
    }

    public Guid GetId() => Guid.Parse("dbb05c98-9112-460f-be89-7fe399cb5a58");

    public Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        return Task.CompletedTask;
    }

    public Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var options = JsonConvert.DeserializeObject<CallLinkRuleOptions>(rule.ParametersJson);

        rule.Name = $"Call {options?.Http}";
        rule.Description = $"Will call  {options?.Http}";

        return Task.CompletedTask;
    }

    internal class CallLinkRuleOptions
    {
        public string Http { get; set; } = string.Empty;
    }
}
