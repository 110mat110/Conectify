namespace Conectify.Shared.Library.Classes;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public interface ISerializable
{
    string ToJson();
}

public class Serializable : ISerializable
{
    public string ToJson()
    {
        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        return JsonConvert.SerializeObject(this, serializerSettings).Replace(@"\", "");
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this).Replace(@"\", "");
    }

}
