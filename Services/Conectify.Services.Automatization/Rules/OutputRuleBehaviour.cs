using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class OutputRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public int DefaultOutputs => 0;

    public IEnumerable<Tuple<InputTypeEnum, int>> DefaultInputs => [new(InputTypeEnum.Value, 1), new(InputTypeEnum.Trigger, 1)];


    public string DisplayName() => "OUTPUT";

    public Guid GetId()
    {
        return Guid.Parse("d274c7f0-211e-413a-8689-f2543dbfc818");
    }

    public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        var options = JsonConvert.DeserializeObject<OutputRuleOptions>(masterRule.ParametersJson);

        var input = masterRule.Inputs.FirstOrDefault();

        if (input is not null && input.AutomatisationValue is not null && options is not null)
        {
            var automatisationValue = input.AutomatisationValue;

            if (automatisationValue is not null)
            {
                using var scope = serviceProvider.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<AutomatizationConfiguration>();

                var command = new WebsocketEvent()
                {
                    DestinationId = options.DestinationId,
                    Name = automatisationValue.Name,
                    NumericValue = automatisationValue.NumericValue,
                    StringValue = automatisationValue.StringValue,
                    TimeCreated = automatisationValue.TimeCreated,
                    Unit = automatisationValue.Unit,
                    SourceId = configuration.SensorId,
                    Type = Constants.Events.Action,
                };

                var wsClient = scope.ServiceProvider.GetRequiredService<IServicesWebsocketClient>();
                wsClient.SendMessageAsync(command, ct);
            }
        }

        return Task.CompletedTask;
    }

    public Task InitializationValue(RuleDTO rule)
    {
        return Task.CompletedTask;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
    }

    internal class OutputRuleOptions
    {
        public Guid DestinationId { get; set; } = Guid.Empty;
    }
}

