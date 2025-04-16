using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Library;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class UserInputRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehavior
{
    public MinMaxDef Outputs => new(1, 1, 1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0,0,0)),
            new(InputTypeEnum.Trigger, new(0,0,0)),
            new(InputTypeEnum.Parameter, new(0,0,0))
        ];

    public AutomatisationEvent? Execute(IEnumerable<AutomatisationEvent> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationEvent>> parameterValues)
    {
        return automatisationValues.FirstOrDefault();
    }

    public AutomatisationEvent? InitializationValue(RuleDTO rule)
    {
        return null;
    }

    public string DisplayName() => "USER INPUT";

    public Guid GetId()
    {
        return Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925798");
    }

    public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    Task IRuleBehavior.InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        throw new NotImplementedException();
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var id = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { SourceActuatorId = Guid.Empty })?.SourceActuatorId;
        if (id is null || id == Guid.Empty)
            return;

        var connectorService = serviceProvider.GetRequiredService<IConnectorService>();

        var actuator = await connectorService.LoadActuator(id!.Value, cancellationToken);
        if (actuator is null)
        {
            return;
        }
        rule.ParametersJson = JsonConvert.SerializeObject(new { SourceActuatorId = actuator.Id, actuator.Name });

        rule.Name = actuator.Name;
        rule.Description = "Source: " + actuator.Name;
    }
}
