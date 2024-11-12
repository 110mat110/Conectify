namespace Conectify.Server.Controllers;

using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Conectify.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class SystemController(ConectifyDb database, IDataService dataService) : ControllerBase
{
    [HttpGet("Ping")]
    public string GetPing()
    {
        return "Hello world";
    }

    [HttpGet("Time")]
    public long GetTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    [HttpGet("UpdateDatabase")]
    public string UpdateDatabase()
    {
        database.Database.Migrate();
        return "database update";
    }

    [HttpPost("{deviceId}/Event")]
    public async Task InsertJson(Guid deviceId, Event evnt)
    {
        await dataService.ProcessEntity(evnt, deviceId);
    }

    [HttpGet("SeedTestData")]
    public async Task<string> SeedData()
    {
        var random = new Random();
        if (await database.Devices.AnyAsync())
        {
            return "Database is not empty!";
        }
        for (int d = 0; d < random.Next(5); d++)
        {
            var deviceId = Guid.NewGuid();
            database.Devices.Add(new Device()
            {
                Id = deviceId,
                IPAdress = "TEST",
                MacAdress = "TEST",
                Name = $"TEST {d}",
                IsKnown = true,
            });

            for (int i = 0; i < random.Next(2, 15); i++)
            {
                var sensorId = Guid.NewGuid();
                database.Sensors.Add(new Sensor()
                {
                    Id = sensorId,
                    Name = $"TEST SENSOR {i}",
                    SourceDeviceId = deviceId,
                    IsKnown = true,
                });

                if (random.Next(3) == 0)
                {
                    database.Actuators.Add(new Actuator()
                    {
                        Id = Guid.NewGuid(),
                        IsKnown = true,
                        Name = $"TEST ACTUATOR {i}",
                        SensorId = sensorId,
                        SourceDeviceId = deviceId,
                    });
                }
                for (int v = 0; v < random.Next(); v++) { }
                database.Events.Add(new Database.Models.Values.Event()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test value",
                    NumericValue = random.NextSingle() * 100,
                    SourceId = sensorId,
                    StringValue = "test",
                    TimeCreated = DateTimeOffset.UtcNow.AddSeconds(random.NextDouble() * 100).ToUnixTimeMilliseconds(),
                    Unit = "Test",
                });
            };
        }
        await database.SaveChangesAsync();
        return "Database seeded";
    }
}
