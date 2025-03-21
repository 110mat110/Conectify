using Conectify.Database;
using Conectify.Services.Library;
using Conectify.Services.Shelly.Models.Shelly;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Newtonsoft.Json;

namespace Conectify.Services.Shelly.Services;

public class ShellyFactory(IConnectorService connectorService, Configuration configuration, ConectifyDb conectifyDb)
{
    private readonly Dictionary<string, Type> shellyTypes = new()
    {
        { "S3SW-001X8EU", typeof(Shelly1G3)},
        { "SNSN-0024X", typeof(ShellyI4)},
        { "S3SW-002P16EU", typeof(Shelly2PMG3) },
        { "SPSW-003XE16EU", typeof(Shelly3Pro) },
        { "SNPM-001PCEU16", typeof(ShellyPmG3) },
    };
    public async Task<IShelly> GetShelly(string model, string id, string name)
    {
        IShelly? shelly = null;

        if (!shellyTypes.TryGetValue(model, out Type? shellyType))
        {
            throw new ArgumentNullException($"We do not support {model}");
        }

        var shellyDb = conectifyDb.Shellys.FirstOrDefault(x => x.ShellyId == id);

        if (shellyDb is not null) {
            shelly = JsonConvert.DeserializeObject(shellyDb.Json, shellyType) as IShelly;
        }


        if (shelly is null)
        {
            object[] parameters = { name, id};
            shelly = Activator.CreateInstance(shellyType, parameters) as IShelly;
        }

        if (shelly is not null)
        {
            await ConectToConectify(shelly);

            var serializedShelly = JsonConvert.SerializeObject(shelly);

            if (shellyDb is null)
            {
                await conectifyDb.Shellys.AddAsync(new Database.Models.Shelly.Shelly()
                {

                    ShellyId = id,
                    Json = serializedShelly
                });
            }
            else
            {
                shellyDb.Json = serializedShelly;
            }

            await conectifyDb.SaveChangesAsync();
        }

        return shelly;
    }

    private async Task ConectToConectify(IShelly shelly)
    {
        List<ApiSensor> sensors = [];
        List<ApiActuator> actuators = [];
        List<Tuple<Guid,MetadataServiceConnector>> metadatas = [];

        foreach (var power in shelly.Powers)
        {
            sensors.Add(new ApiSensor()
            {
                Id = power.SensorId,
                Name = $"{shelly.Name} PWR-{power.ShellyId}",
                SourceDeviceId = configuration.DeviceId,
            });
        }

        foreach(var sw in shelly.Switches)
        {
            sensors.Add(new ApiSensor()
            {
                Id = sw.SensorId,
                Name = $"{shelly.Name} SW-{sw.ShellyId}",
                SourceDeviceId = configuration.DeviceId,
            });
            actuators.Add(new ApiActuator()
            {
                Id = sw.ActuatorId,
                SensorId = sw.SensorId,
                Name = $"{shelly.Name} SW-{sw.ShellyId}",
                SourceDeviceId = configuration.DeviceId
            });
            metadatas.Add(new (sw.SensorId, new MetadataServiceConnector()
            {
                MaxVal = 101,
                MinVal = 60,
                StringValue = "#ccccc",
                MetadataName = "Threshold"
            }));
            if (sw.Power is not null)
            {
                sensors.Add(new ApiSensor()
                {
                    Id = sw.Power.SensorId,
                    Name = $"{shelly.Name} SW-{sw.ShellyId} Power",
                    SourceDeviceId = configuration.DeviceId
                });
            }
        }

        foreach(var sw in shelly.DetachedInputs)
        {
            sensors.Add(new ApiSensor()
            {
                Id = sw.SensorId,
                Name = $"{shelly.Name} DI-{sw.ShellyId}",
                SourceDeviceId = configuration.DeviceId
            });
        }

        await connectorService.RegisterSensors(sensors);
        await connectorService.RegisterActuators(actuators);
        await connectorService.SendMetadataForSensor(metadatas);
    }
}

public class ShellyData
{
    public Dictionary<string, string> shellyData = new();
}
