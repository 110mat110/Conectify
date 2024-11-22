using Conectify.Services.Automatization.Models.Database;

namespace Conectify.Services.Automatization.Models.DTO;

public class InputPointDTO
{
    public Guid Id { get; set; }

    public InputTypeEnum Type { get; set; }

    public AutomatisationEvent? AutomatisationValue { get; set; }

    public RuleDTO Rule { get; set; } = null!;

    public async Task SetEvent(AutomatisationEvent automatisationEvent, bool trigger, CancellationToken ct)
    {
        AutomatisationValue = automatisationEvent;

        if (trigger && (Rule.TriggerOnValue || Type == InputTypeEnum.Trigger))
        {
            await Rule.OnTrigger(automatisationEvent, ct);
        }
    }
}
