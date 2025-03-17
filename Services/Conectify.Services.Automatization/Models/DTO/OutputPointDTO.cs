﻿using Conectify.Services.Automatization.Services;

namespace Conectify.Services.Automatization.Models.DTO;

public class OutputPointDTO(Guid id, IServiceProvider serviceProvider)
{
    public Guid Id => id;

    public int Index { get; set; }
    public AutomatisationEvent? Event { get; set; } = null;

    public async Task SetOutputEvent(AutomatisationEvent? automatisationEvent, bool trigger = true, CancellationToken ct = default)
    {
        Event = automatisationEvent;

        if (trigger && automatisationEvent != null)
        {
            var cache = serviceProvider.GetRequiredService<IAutomatizationCache>();

            var inputs = await cache.GetNextInputs(Id);
            foreach (var input in inputs)
            {
                await input.SetEvent(automatisationEvent, ct);
            }
        }
    }
}
