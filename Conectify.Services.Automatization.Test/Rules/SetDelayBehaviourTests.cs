using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

public class SetDelayBehaviourTests
{
    [Fact]
    public async Task Execute_ShouldSetNextTrigger()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new RuleDTO
        {
            ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromSeconds(5), NextTrigger = (DateTime?)null })
        };

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        var options = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { Delay = TimeSpan.Zero, NextTrigger = (DateTime?)null });
        Assert.NotNull(options);
        Assert.NotNull(options!.NextTrigger);
    }

    [Fact]
    public async Task SetParameters_WithDelay_ShouldSetDescription()
    {
        var behaviour = new SetDelayBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { Delay = TimeSpan.FromMinutes(1) })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Delay", rule.Description);
    }
}
