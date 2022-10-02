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
        var type = JsonConvert.DeserializeAnonymousType(rawJson, new { Type = string.Empty });
        var expectedTypeName = (type != null ? type.Type : string.Empty);
        var websocketTypeName = "Websocket" + expectedTypeName;
        Assembly asm = typeof(Value).Assembly;
        Type? t = asm.GetTypes().FirstOrDefault(x => x.Name.ToLower() == expectedTypeName.ToLower());

        Assembly wasm = typeof(WebsocketValue).Assembly;
        Type? wt = wasm.GetTypes().FirstOrDefault(x => x.Name.ToLower() == websocketTypeName.ToLower());

        if (t is null || wt is null)
        {
            throw new ConectifyException($"Does not recognize type {(type != null ? type.Type : string.Empty)}");
        }

        var deserialized = JsonConvert.DeserializeObject(rawJson, wt);

        if (deserialized is null)
        {
            throw new ConectifyException($"Could not serialize {rawJson} to type {wt.Name}");
        }

        var mapped = mapper.Map(deserialized, wt, t) as IBaseInputType;

        if (mapped is null)
        {
            throw new ConectifyException($"Could not map {wt.Name} to {t.Name}");

        }

        return (mapped, t);
    }
}
