using Conectify.Services.Automatization.Controllers;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Automatization.Test.Controllers;

public class BehaviourControllerTests
{
    private readonly IServiceProvider serviceProvider;
    private readonly BehaviourController controller;

    public BehaviourControllerTests()
    {
        var services = new ServiceCollection();
        serviceProvider = services.BuildServiceProvider();
        controller = new BehaviourController(serviceProvider);
    }

    [Fact]
    public void GetAllBehaviours_ReturnsAllBehaviours()
    {
        var result = controller.GetAllBehaviours();

        Assert.NotEmpty(result);
        Assert.All(result, behaviour =>
        {
            Assert.NotEqual(Guid.Empty, behaviour.Id);
            Assert.NotNull(behaviour.Name);
        });
    }

    [Fact]
    public void GetAllBehaviours_ContainsKnownBehaviours()
    {
        var result = controller.GetAllBehaviours().ToList();

        Assert.Contains(result, b => b.Id == new AndRuleBehaviour(default).GetId());
        Assert.Contains(result, b => b.Id == new InputRuleBehaviour(default).GetId());
        Assert.Contains(result, b => b.Id == new OutputRuleBehaviour(default).GetId());
        Assert.Contains(result, b => b.Id == new RunAtRuleBehaviour(default).GetId());
    }

    [Fact]
    public void GetBehaviour_ValidId_ReturnsBehaviour()
    {
        var andBehaviourId = new AndRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(andBehaviourId);

        Assert.NotNull(result);
        Assert.Equal(andBehaviourId, result.Id);
        Assert.Equal("AND", result.Name);
    }

    [Fact]
    public void GetBehaviour_InvalidId_ThrowsException()
    {
        var invalidId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        Assert.Throws<Exception>(() => controller.GetBehaviour(invalidId));
    }

    [Fact]
    public void GetBehaviour_AndBehaviour_HasCorrectInputsAndOutputs()
    {
        var andBehaviourId = new AndRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(andBehaviourId);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Inputs);
        Assert.NotNull(result.Outputs);
        Assert.Equal(1, result.Outputs.Min);
        Assert.Equal(1, result.Outputs.Max);
    }

    [Fact]
    public void GetBehaviour_RunAtBehaviour_HasCorrectConfiguration()
    {
        var runAtBehaviourId = new RunAtRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(runAtBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("RUN AT", result.Name);
        Assert.Equal(1, result.Outputs.Min);
        Assert.Equal(1, result.Outputs.Max);
    }

    [Fact]
    public void GetBehaviour_InputBehaviour_HasCorrectConfiguration()
    {
        var inputBehaviourId = new InputRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(inputBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("INPUT", result.Name);
    }

    [Fact]
    public void GetBehaviour_OutputBehaviour_HasCorrectConfiguration()
    {
        var outputBehaviourId = new OutputRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(outputBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("OUTPUT", result.Name);
    }

    [Fact]
    public void GetAllBehaviours_ReturnsUniqueIds()
    {
        var result = controller.GetAllBehaviours().ToList();

        var ids = result.Select(b => b.Id).ToList();
        var distinctIds = ids.Distinct().ToList();

        Assert.Equal(ids.Count, distinctIds.Count);
    }

    [Fact]
    public void GetAllBehaviours_AllHaveValidOutputConfiguration()
    {
        var result = controller.GetAllBehaviours();

        Assert.All(result, behaviour =>
        {
            Assert.NotNull(behaviour.Outputs);
            Assert.True(behaviour.Outputs.Min >= 0);
            Assert.True(behaviour.Outputs.Max >= behaviour.Outputs.Min);
            Assert.True(behaviour.Outputs.Def >= behaviour.Outputs.Min);
            Assert.True(behaviour.Outputs.Def <= behaviour.Outputs.Max);
        });
    }

    [Fact]
    public void GetBehaviour_DecisionBehaviour_HasCorrectConfiguration()
    {
        var decisionBehaviourId = new DecisionRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(decisionBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("DECISION", result.Name);
    }

    [Fact]
    public void GetBehaviour_SetValueBehaviour_HasCorrectConfiguration()
    {
        var setValueBehaviourId = new SetValueRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(setValueBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("SET VALUE", result.Name);
    }

    [Fact]
    public void GetBehaviour_UserInputBehaviour_HasCorrectConfiguration()
    {
        var userInputBehaviourId = new UserInputRuleBehaviour(default).GetId();

        var result = controller.GetBehaviour(userInputBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("USER INPUT", result.Name);
    }

    [Fact]
    public void GetBehaviour_SetDelayBehaviour_HasCorrectConfiguration()
    {
        var setDelayBehaviourId = new SetDelayBehaviour(default).GetId();

        var result = controller.GetBehaviour(setDelayBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("SET DELAY", result.Name);
    }

    [Fact]
    public void GetBehaviour_CallLinkBehaviour_HasCorrectConfiguration()
    {
        var callLinkBehaviourId = new CallLinkRuleBehavior(default).GetId();

        var result = controller.GetBehaviour(callLinkBehaviourId);

        Assert.NotNull(result);
        Assert.Equal("Call HTTP", result.Name);
    }
}
