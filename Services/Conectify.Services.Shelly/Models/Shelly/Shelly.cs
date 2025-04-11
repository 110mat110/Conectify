﻿using Conectify.Database.Models;
using Conectify.Services.Shelly.Services;

namespace Conectify.Services.Shelly.Models.Shelly;

public interface IShelly
{
    public string Name { get; set; }
    public string Id { get; set; }
    public List<Switch> Switches { get; set; }
    public List<DetachedInput> DetachedInputs { get; set; }
    public List<Power> Powers { get; set; }

}

public class Shelly() : IShelly
{
    public string Name { get; set; } = string.Empty;
    public List<Switch> Switches { get; set; } = [];
    public List<DetachedInput> DetachedInputs { get; set; } = [];
    public List<Power> Powers { get; set; } = [];

    public string Id { get; set; } = null!;
}

public class Switch
{
    public int ShellyId { get; set; }

    public Guid ActuatorId { get; set; }

    public Guid SensorId { get; set; }
    public Power? Power { get; set; }
}

public class DetachedInput
{
    public int ShellyId { get; set; }
    public Guid SensorId { get; set; }
}

public class Power
{
    public int ShellyId { get; set; }
    public Guid SensorId { get; set; }
}