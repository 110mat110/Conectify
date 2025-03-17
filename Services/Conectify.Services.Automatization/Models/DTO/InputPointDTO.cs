using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Services;

namespace Conectify.Services.Automatization.Models.DTO;

public class InputPointDTO
{
    public Guid Id { get; set; }

    public int Index { get; set; }

    public InputTypeEnum Type { get; set; }

    public RuleDTO Rule { get; set; } = null!;

    public async Task<AutomatisationEvent?> GetEvent(IServiceProvider serviceProvider)
    {
        var cache = serviceProvider.GetRequiredService<IAutomatizationCache>();
        var outputs = await cache.GetPreviousOutputs(Id);

        return outputs.FirstOrDefault()?.Event;
    }

    public async Task SetEvent(AutomatisationEvent automatisationEvent, CancellationToken ct)
    {
        if ((Rule.TriggerOnValue || Type == InputTypeEnum.Trigger))
        {
            await Rule.OnTrigger(automatisationEvent, ct);
        }
    }
}
