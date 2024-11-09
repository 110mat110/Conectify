namespace Conectify.Shared.Maps;

using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<WebsocketEvent, Event>();
        CreateMap<ApiEvent, Event>();
        CreateMap<Event, ApiEvent>();
        CreateMap<Event, WebsocketEvent>();
    }
}
