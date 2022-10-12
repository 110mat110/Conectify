using AutoMapper;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conectify.Shared.Maps;

public class PreferenceProfile : Profile
{
    public PreferenceProfile()
    {
        CreateMap<ApiPreference, Preference>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Actuator, opt => opt.Ignore())
            .ForMember(x => x.Sensor, opt => opt.Ignore())
            .ForMember(x => x.Subscriber, opt => opt.Ignore())
            .ForMember(x => x.SubscriberId, opt => opt.Ignore())
            .ForMember(x => x.Device, opt => opt.Ignore());
    }
}
