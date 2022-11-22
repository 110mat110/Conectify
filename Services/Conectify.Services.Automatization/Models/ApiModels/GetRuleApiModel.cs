namespace Conectify.Services.Automatization.Models.ApiModels;

public class GetRuleApiModel
{
    public Guid Id { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public string PropertyJson { get; set; } = string.Empty;

    public Guid BehaviourId { get; set; }

    public IEnumerable<Guid> Targets { get; set; } = new List<Guid>();
}
