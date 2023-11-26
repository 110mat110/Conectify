namespace Conectify.Services.Dashboard.Models;

public record AddDeviceApi(Guid DeviceId, int PosX, int PosY, string SourceType);