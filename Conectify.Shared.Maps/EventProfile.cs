namespace Conectify.Shared.Maps;

using Conectify.Database.Models.Values;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;

public class EventProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        configuration.CreateMap<WebsocketEvent, Event>(x => new Event
        {
            Id = x.Id,
            SourceId = x.SourceId,
            DestinationId = x.DestinationId,
            Type = x.Type,
            Name = x.Name,
            Unit = x.Unit,
            StringValue = x.StringValue,
            NumericValue = x.NumericValue,
            TimeCreated = x.TimeCreated,
        });

        configuration.CreateMap<ApiEvent, Event>(x => new Event
        {
            Id = x.Id,
            SourceId = x.SourceId,
            DestinationId = x.DestinationId,
            Type = x.Type,
            Name = x.Name,
            Unit = x.Unit,
            StringValue = x.StringValue,
            NumericValue = x.NumericValue,
            TimeCreated = x.TimeCreated,
        });

        configuration.CreateMap<Event, ApiEvent>(x => new ApiEvent
        {
            Id = x.Id,
            SourceId = x.SourceId,
            DestinationId = x.DestinationId,
            Type = x.Type,
            Name = x.Name,
            Unit = x.Unit,
            StringValue = x.StringValue,
            NumericValue = x.NumericValue,
            TimeCreated = x.TimeCreated,
        });

        configuration.CreateMap<Event, WebsocketEvent>(x => new WebsocketEvent
        {
            Id = x.Id,
            SourceId = x.SourceId,
            DestinationId = x.DestinationId,
            Type = x.Type,
            Name = x.Name,
            Unit = x.Unit,
            StringValue = x.StringValue,
            NumericValue = x.NumericValue,
            TimeCreated = x.TimeCreated,
        });
    }
}
