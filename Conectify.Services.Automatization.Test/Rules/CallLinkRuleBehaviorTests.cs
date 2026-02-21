using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Rules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

public class CallLinkRuleBehaviorTests
{
    [Fact]
    public void GetId_ShouldReturnCorrectGuid()
    {
        var behaviour = new CallLinkRuleBehavior(new ServiceCollection().BuildServiceProvider());

        Assert.Equal(Guid.Parse("dbb05c98-9112-460f-be89-7fe399cb5a58"), behaviour.GetId());
    }

    [Fact]
    public void DisplayName_ShouldReturnCallHttp()
    {
        var behaviour = new CallLinkRuleBehavior(new ServiceCollection().BuildServiceProvider());

        Assert.Equal("Call HTTP", behaviour.DisplayName());
    }

    [Fact]
    public async Task SetParameters_WithHttpUrl_ShouldSetNameAndDescription()
    {
        var behaviour = new CallLinkRuleBehavior(new ServiceCollection().BuildServiceProvider());
        var httpUrl = "https://example.com/api";
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { Http = httpUrl })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Equal($"Call {httpUrl}", rule.Name);
        Assert.Equal($"Will call  {httpUrl}", rule.Description);
    }
}
