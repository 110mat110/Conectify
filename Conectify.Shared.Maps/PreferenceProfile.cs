using Conectify.Database.Models;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Library.Models;

namespace Conectify.Shared.Maps;

public class PreferenceProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        configuration.CreateMap<ApiPreference, Preference>(
            x => new Preference
            {
                SubscibeeId = x.SubscibeeId,
                EventType = x.EventType,
            },
            nameof(Preference.Id), nameof(Preference.SubscriberId), nameof(Preference.Subscriber));
    }
}
