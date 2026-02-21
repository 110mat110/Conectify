using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Automatization.Test.Services;

public class BehaviourFactoryTests
{
    private readonly IServiceProvider serviceProvider;

    public BehaviourFactoryTests()
    {
        var services = new ServiceCollection();
        serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_AndBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new AndRuleBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<AndRuleBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_InputBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new InputRuleBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<InputRuleBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_OutputBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new OutputRuleBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<OutputRuleBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_RunAtBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new RunAtRuleBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<RunAtRuleBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_DecisionBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new DecisionRuleBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<DecisionRuleBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_SetValueBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new SetValueRuleBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<SetValueRuleBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_UserInputBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new UserInputRuleBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<UserInputRuleBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_SetDelayBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new SetDelayBehaviour(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<SetDelayBehaviour>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_CallLinkBehaviour_ReturnsCorrectBehaviour()
    {
        var behaviourId = new CallLinkRuleBehavior(default).GetId();

        var result = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotNull(result);
        Assert.IsType<CallLinkRuleBehavior>(result);
        Assert.Equal(behaviourId, result.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_InvalidId_ThrowsException()
    {
        var invalidId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var exception = Assert.Throws<Exception>(() => 
            BehaviorFactory.GetRuleBehaviorByTypeId(invalidId, serviceProvider));

        Assert.Equal($"Cannot load behaviour {invalidId}", exception.Message);
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_EmptyGuid_ThrowsException()
    {
        var emptyGuid = Guid.Empty;

        var exception = Assert.Throws<Exception>(() =>
            BehaviorFactory.GetRuleBehaviorByTypeId(emptyGuid, serviceProvider));

        Assert.Equal($"Cannot load behaviour {emptyGuid}", exception.Message);
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_AllBehavioursHaveUniqueIds()
    {
        var behaviours = new List<IRuleBehavior>
        {
            new AndRuleBehaviour(serviceProvider),
            new InputRuleBehaviour(serviceProvider),
            new OutputRuleBehaviour(serviceProvider),
            new RunAtRuleBehaviour(serviceProvider),
            new DecisionRuleBehaviour(serviceProvider),
            new SetValueRuleBehaviour(serviceProvider),
            new UserInputRuleBehaviour(serviceProvider),
            new SetDelayBehaviour(serviceProvider),
            new CallLinkRuleBehavior(serviceProvider)
        };

        var ids = behaviours.Select(b => b.GetId()).ToList();
        var distinctIds = ids.Distinct().ToList();

        Assert.Equal(ids.Count, distinctIds.Count);
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_ReturnsNewInstanceEachTime()
    {
        var behaviourId = new AndRuleBehaviour(default).GetId();

        var result1 = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);
        var result2 = BehaviorFactory.GetRuleBehaviorByTypeId(behaviourId, serviceProvider);

        Assert.NotSame(result1, result2);
        Assert.Equal(result1.GetId(), result2.GetId());
    }

    [Fact]
    public void GetRuleBehaviorByTypeId_AllBehavioursHaveDisplayNames()
    {
        var behaviours = new List<IRuleBehavior>
        {
            new AndRuleBehaviour(serviceProvider),
            new InputRuleBehaviour(serviceProvider),
            new OutputRuleBehaviour(serviceProvider),
            new RunAtRuleBehaviour(serviceProvider),
            new DecisionRuleBehaviour(serviceProvider),
            new SetValueRuleBehaviour(serviceProvider),
            new UserInputRuleBehaviour(serviceProvider),
            new SetDelayBehaviour(serviceProvider),
            new CallLinkRuleBehavior(serviceProvider)
        };

        Assert.All(behaviours, behaviour =>
        {
            Assert.NotNull(behaviour.DisplayName());
            Assert.NotEmpty(behaviour.DisplayName());
        });
    }
}
