using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class OutputRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public MinMaxDef Outputs => new(0, 0, 0);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(1,1,1)),
            new(InputTypeEnum.Trigger, new(0,1,1)),
            new(InputTypeEnum.Parameter, new(0,0,0))
        ];

    public string DisplayName() => "OUTPUT";

    public Guid GetId()
    {
        return Guid.Parse("d274c7f0-211e-413a-8689-f2543dbfc818");
    }

    public async Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        var options = JsonConvert.DeserializeObject<OutputRuleOptions>(masterRule.ParametersJson);

        if (triggerValue is not null)
        {
                using var scope = serviceProvider.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<AutomatizationConfiguration>();

                var command = new WebsocketEvent()
                {
                    DestinationId = options?.DestinationId,
                    Name = triggerValue.Name,
                    NumericValue = triggerValue.NumericValue,
                    StringValue = triggerValue.StringValue,
                    TimeCreated = triggerValue.TimeCreated,
                    Unit = triggerValue.Unit,
                    SourceId = configuration.SensorId,
                    Type = Constants.Events.Action,
                };

                var wsClient = scope.ServiceProvider.GetRequiredService<IServicesWebsocketClient>();
                await wsClient.SendMessageAsync(command, ct);
            }
    }

    public Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        return Task.CompletedTask;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
    }

    public async Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var id = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { DestinationId = Guid.Empty })?.DestinationId;
        if (id is null || id == Guid.Empty)
            return;
        var connectorService = serviceProvider.GetRequiredService<IConnectorService>();
        var actuator = await connectorService.LoadActuator(id!.Value, cancellationToken);
        if (actuator is null)
        {
            return;
        }
        rule.ParametersJson = JsonConvert.SerializeObject(new { DestinationId = actuator.Id, actuator.Name });

        rule.Name = actuator.Name;
        rule.Description = "Destintaion: " + actuator.Name;
    }

    internal class OutputRuleOptions
    {
        public Guid DestinationId { get; set; } = Guid.Empty;
    }
}

