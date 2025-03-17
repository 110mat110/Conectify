namespace Conectify.Services.Automatization.Models.ApiModels;

public record EditRuleApiModel
{
    public Guid Id { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public string Parameters { get; set; } = string.Empty;

    public Guid BehaviourId { get; set; }

    public bool TriggerOnEvent { get; set; }
}
