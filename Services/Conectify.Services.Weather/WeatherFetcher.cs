using System.Text.Json;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;

namespace Conectify.Services.Weather;

public class WeatherFetcher(IServicesWebsocketClient websocketClient, Configuration configuration, ILogger<WeatherFetcher> logger)
{
    private static readonly HttpClient httpClient = new();

    public async Task LoadNewValues()
    {
        await LoadCurrentWeather();
        await LoadWarnings();
    }

    private async Task LoadCurrentWeather()
    {
        try
        {
            var url = $"https://api.open-meteo.com/v1/forecast" +
                      $"?latitude={configuration.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&longitude={configuration.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&current=temperature_2m,wind_speed_10m,wind_direction_10m,rain,shortwave_radiation" +
                      $"&hourly=precipitation&forecast_days=2&wind_speed_unit=ms&timezone=UTC";

            using var response = JsonDocument.Parse(await httpClient.GetStringAsync(url));
            var current = response.RootElement.GetProperty("current");

            await SendValue(configuration.TemperatureSensorId, "Temperature", current.GetProperty("temperature_2m").GetSingle(), "°C");
            await SendValue(configuration.WindSpeedSensorId, "Wind speed", current.GetProperty("wind_speed_10m").GetSingle(), "m/s");
            await SendValue(configuration.WindDirectionSensorId, "Wind direction", current.GetProperty("wind_direction_10m").GetSingle(), "°");
            await SendValue(configuration.SunIntensitySensorId, "Sun intensity", current.GetProperty("shortwave_radiation").GetSingle(), "W/m²");
            await SendValue(configuration.RainSensorId, "Rain", current.GetProperty("rain").GetSingle(), "mm");

            var (azimuth, elevation) = GetSunPosition(DateTimeOffset.UtcNow, configuration.Latitude, configuration.Longitude);
            await SendValue(configuration.SunAzimuthSensorId, "Sun azimuth", (float)azimuth, "°");
            await SendValue(configuration.SunElevationSensorId, "Sun elevation", (float)elevation, "°");

            var forecast = GetRainForecast24h(response.RootElement);
            await SendValue(configuration.RainForecastSensorId, "Rain forecast 24h", forecast, "mm");

            logger.LogInformation("Weather updated, temperature {Temperature} °C, rain next 24h {Forecast} mm",
                current.GetProperty("temperature_2m").GetSingle(), forecast);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load current weather");
        }
    }

    private static float GetRainForecast24h(JsonElement root)
    {
        var hourly = root.GetProperty("hourly");
        var times = hourly.GetProperty("time").EnumerateArray().ToList();
        var precipitation = hourly.GetProperty("precipitation").EnumerateArray().ToList();

        var now = DateTime.UtcNow;
        float sum = 0;
        var counted = 0;
        for (var i = 0; i < times.Count && counted < 24; i++)
        {
            var time = DateTime.Parse(times[i].GetString()!, System.Globalization.CultureInfo.InvariantCulture);
            if (time < now.AddHours(-1))
            {
                continue;
            }

            if (precipitation[i].ValueKind == JsonValueKind.Number)
            {
                sum += precipitation[i].GetSingle();
            }
            counted++;
        }

        return sum;
    }

    private async Task LoadWarnings()
    {
        try
        {
            using var response = JsonDocument.Parse(await httpClient.GetStringAsync("https://feeds.meteoalarm.org/api/v1/warnings/feeds-czechia"));
            var warnings = new List<string>();

            foreach (var warning in response.RootElement.GetProperty("warnings").EnumerateArray())
            {
                if (!warning.TryGetProperty("alert", out var alert) || !alert.TryGetProperty("info", out var infos))
                {
                    continue;
                }

                foreach (var info in infos.EnumerateArray())
                {
                    if (info.TryGetProperty("language", out var language) && !language.GetString()!.StartsWith("cs", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!IsForRegion(info) || !IsActive(info))
                    {
                        continue;
                    }

                    var eventName = info.TryGetProperty("event", out var ev) ? ev.GetString() : "Unknown";
                    var severity = info.TryGetProperty("severity", out var sev) ? sev.GetString() : string.Empty;
                    warnings.Add($"{eventName} ({severity})");
                }
            }

            await SendValue(configuration.WarningSensorId, "Weather warnings", warnings.Count, string.Empty, string.Join("; ", warnings.Distinct()));
            logger.LogInformation("Loaded {Count} weather warnings for region {Region}", warnings.Count, configuration.WarningRegion);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load weather warnings");
        }
    }

    private static bool IsActive(JsonElement info)
    {
        // the feed also carries expired warnings and "all clear" notices
        if (info.TryGetProperty("responseType", out var responseTypes) &&
            responseTypes.EnumerateArray().Any(rt => rt.GetString() == "AllClear"))
        {
            return false;
        }

        if (info.TryGetProperty("expires", out var expires) &&
            DateTimeOffset.TryParse(expires.GetString(), out var expiresAt) &&
            expiresAt < DateTimeOffset.UtcNow)
        {
            return false;
        }

        return true;
    }

    private bool IsForRegion(JsonElement info)
    {
        if (!info.TryGetProperty("area", out var areas))
        {
            return false;
        }

        return areas.EnumerateArray().Any(area =>
            area.TryGetProperty("areaDesc", out var desc) &&
            desc.GetString()!.Contains(configuration.WarningRegion, StringComparison.OrdinalIgnoreCase));
    }

    private async Task SendValue(Guid sensorId, string name, float numericValue, string unit, string stringValue = "")
    {
        var value = new WebsocketEvent()
        {
            Name = name,
            NumericValue = numericValue,
            StringValue = stringValue,
            SourceId = sensorId,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Unit = unit,
            Type = Constants.Events.Value,
        };
        await websocketClient.SendMessageAsync(value);
    }

    private static (double Azimuth, double Elevation) GetSunPosition(DateTimeOffset time, double latitude, double longitude)
    {
        // NOAA solar position approximation, accurate to a fraction of a degree
        var d = time.UtcDateTime.ToOADate() + 2415018.5 - 2451545.0;
        var meanAnomaly = DegToRad((357.529 + 0.98560028 * d) % 360);
        var meanLongitude = (280.459 + 0.98564736 * d) % 360;
        var eclipticLongitude = DegToRad((meanLongitude + 1.915 * Math.Sin(meanAnomaly) + 0.020 * Math.Sin(2 * meanAnomaly)) % 360);
        var obliquity = DegToRad(23.439 - 0.00000036 * d);

        var rightAscension = Math.Atan2(Math.Cos(obliquity) * Math.Sin(eclipticLongitude), Math.Cos(eclipticLongitude));
        var declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitude));

        var siderealTime = ((18.697374558 + 24.06570982441908 * d) % 24) * 15 + longitude;
        var hourAngle = DegToRad(siderealTime) - rightAscension;

        var latRad = DegToRad(latitude);
        var elevation = Math.Asin(Math.Sin(latRad) * Math.Sin(declination) + Math.Cos(latRad) * Math.Cos(declination) * Math.Cos(hourAngle));
        var azimuth = Math.Atan2(Math.Sin(hourAngle), Math.Cos(hourAngle) * Math.Sin(latRad) - Math.Tan(declination) * Math.Cos(latRad));

        return ((RadToDeg(azimuth) + 180 + 360) % 360, RadToDeg(elevation));
    }

    private static double DegToRad(double degrees) => degrees * Math.PI / 180;
    private static double RadToDeg(double radians) => radians * 180 / Math.PI;
}
