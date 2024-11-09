﻿using Conectify.Services.Library;
using Conectify.Services.Shelly.Models.Shelly;
using Conectify.Shared.Library.Models;
using Newtonsoft.Json;
using System.Transactions;

namespace Conectify.Services.Shelly.Services;

public class ShellyFactory(IConnectorService connectorService, Configuration configuration)
{
    private readonly Dictionary<string, Type> shellyTypes = new()
    {
        { "S3SW-001X8EU", typeof(Shelly1G3)},
        { "SNSN-0024X", typeof(ShellyI4)},
        { "S3SW-002P16EU", typeof(Shelly2PMG3) },
        { "SPSW-003XE16EU", typeof(Shelly3Pro) }
    };
    public IShelly GetShelly(string model, string id, string name)
    {
        IShelly? shelly = null;

        if (!shellyTypes.TryGetValue(model, out Type? shellyType))
        {
            throw new ArgumentNullException($"We do not support {model}");
        }


        var jsonString = File.ReadAllText("Shelly.json");
        ShellyData? database = JsonConvert.DeserializeObject<ShellyData>(jsonString) ?? throw new ArgumentNullException("Database does not work");
        if (database.shellyData.TryGetValue(id, out string? shellyJson))
        {
            shelly = JsonConvert.DeserializeObject(shellyJson,shellyType) as IShelly;
        }

        if(shelly is null)
        {
            object[] parameters = { name, id};
            shelly = Activator.CreateInstance(shellyType, parameters) as IShelly;
        }

        if (shelly is not null)
        {
            ConectToConectify(shelly);

            var serializedShelly = JsonConvert.SerializeObject(shelly);

            if (!database.shellyData.TryAdd(id, serializedShelly))
            {
                database.shellyData[id] = serializedShelly;
            }

            File.WriteAllText("Shelly.json", JsonConvert.SerializeObject(database));
        }

        return shelly;
    }

    private void ConectToConectify(IShelly shelly)
    {
        List<ApiSensor> sensors = [];
        List<ApiActuator> actuators = [];
        foreach(var power in shelly.Powers)
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
                SourceDeviceId = configuration.DeviceId
            });
            actuators.Add(new ApiActuator()
            {
                Id = sw.ActuatorId,
                SensorId = sw.SensorId,
                Name = $"{shelly.Name} SW-{sw.ShellyId}",
                SourceDeviceId = configuration.DeviceId
            });

            if(sw.Power is not null)
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

        connectorService.RegisterSensors(sensors);
        connectorService.RegisterActuators(actuators);
    }
}

public class ShellyData
{
    public Dictionary<string, string> shellyData = new();
}
