using Conectify.Database.Interfaces;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.ErrorHandling;
using Newtonsoft.Json;
using System.Reflection;

namespace Conectify.Shared.Services.Data
{
    public class SharedDataService
    {
        public static ( IBaseInputType,Type ) DeserializeJson(string rawJson)
        {
            var type = JsonConvert.DeserializeAnonymousType(rawJson, new { Type = string.Empty });
            var expectedTypeName = (type != null ? type.Type : string.Empty);
            Assembly asm = typeof(Value).Assembly;
            Type? t = asm.GetTypes().FirstOrDefault(x => x.Name.ToLower() == expectedTypeName.ToLower());

            if (t is null)
            {
                throw new ConectifyException($"Does not recognize type {(type != null ? type.Type : string.Empty)}");
            }

            var deserialized = JsonConvert.DeserializeObject(rawJson, t) as IBaseInputType;

            if (deserialized is null)
            {
                throw new ConectifyException($"Could not serialize {rawJson} to type {t.Name}");
            }

            return (deserialized, t);
        }
    }
}
