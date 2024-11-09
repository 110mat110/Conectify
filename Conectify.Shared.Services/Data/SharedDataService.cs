using AutoMapper;
using Conectify.Database.Interfaces;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.ErrorHandling;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;
using System.Reflection;

namespace Conectify.Shared.Services.Data;

public class SharedDataService
{
    public static Event? DeserializeJson(string rawJson, IMapper mapper)
    {
        var expectedTypeName = string.Empty;
        try
        {
            return JsonConvert.DeserializeObject<Event>(rawJson);
        }
        catch (Exception)
        {
            throw new ConectifyException("Json to deserialize is not an event!!");
        };
    }

    public static Guid? ExtractSourceId(string rawJson)
    {
        try
        {
            var sSensor = JsonConvert.DeserializeAnonymousType(rawJson, new { SourceSensorId = Guid.Empty });
             if(sSensor != null && sSensor.SourceSensorId != Guid.Empty)
                return sSensor.SourceSensorId;

            var sActuator = JsonConvert.DeserializeAnonymousType(rawJson, new { SourceActuatorId = Guid.Empty });
            return sActuator?.SourceActuatorId;
        }
        catch (Exception)
        {
            return null;
        };
    }

    public static Guid? ExtractDestinationId(string rawJson)
    {
        try
        {
            var id = JsonConvert.DeserializeAnonymousType(rawJson, new { DestinationId = Guid.Empty });
            return id?.DestinationId;
        }
        catch (Exception)
        {
            return null;
        };
    }
}
