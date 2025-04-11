﻿using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.OccupancyCheck;

public class DeviceData(Configuration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Occupancy check"
    };

    public IEnumerable<ApiSensor> Sensors =>
    [
        new()
        {
            Id = configuration.SensorId,
            Name = "Occupancy",
            SourceDeviceId = configuration.DeviceId,
        }
    ];

    public IEnumerable<ApiPreference> Preferences => [];

    public IEnumerable<MetadataServiceConnector> MetadataConnectors =>
    [
        new()
        {
            MaxVal = 1,
            MinVal = 0,
            MetadataName = "Visible",
            NumericValue = 0,
            StringValue = string.Empty,
            TypeValue = 0,
            Unit = string.Empty,
        }
    ];

    public IEnumerable<ApiActuator> Actuators => [];
}
