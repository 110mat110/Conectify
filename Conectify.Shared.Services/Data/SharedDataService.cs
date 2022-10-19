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
    public static (IBaseInputType, Type) DeserializeJson(string rawJson, IMapper mapper)
    {
        var expectedTypeName = string.Empty;
        try
        {
            var type = JsonConvert.DeserializeAnonymousType(rawJson, new { Type = string.Empty });
            expectedTypeName = type?.Type;
        } catch (Exception)
        {
            throw new ConectifyException("Json to deserialize is not in valid format!!");
        };
        var websocketTypeName = "Websocket" + expectedTypeName;
        Assembly asm = typeof(Value).Assembly;
        Type? t = asm.GetTypes().FirstOrDefault(x => x.Name.ToLower() == expectedTypeName?.ToLower());

        if (t is null)
        {
            throw new ConectifyException($"Does not recognize type {(expectedTypeName ?? string.Empty)}");
        }

        var deserialized = JsonConvert.DeserializeObject<WebsocketBaseModel>(rawJson);

        if (deserialized is null)
        {
            throw new ConectifyException($"Could not serialize {rawJson} to websocket");
        }

        if (mapper.Map(deserialized,typeof(WebsocketBaseModel), t) is not IBaseInputType mapped)
        {
            throw new ConectifyException($"Could not map websocket to {t.Name}");
        }

        return (mapped, t);
    }
}
