using Conectify.Services.Automatization.Services;

namespace Conectify.Services.Automatization.Models.DTO;

public class OutputPointDTO(Guid id, IServiceProvider serviceProvider)
{
    public Guid Id => id;


    public async Task SetOutputEvent(AutomatisationEvent automatisationEvent, bool trigger = true, CancellationToken ct = default)
    {
        var cache = serviceProvider.GetRequiredService<IAutomatizationCache>();

        var inputs = await cache.GetNextInputs(Id);
        foreach (var input in inputs)
        {
            await input.SetEvent(automatisationEvent, trigger, ct);
        }
    }
}
