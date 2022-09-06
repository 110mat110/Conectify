using Conectify.Shared.Library.Models;

namespace Conectify.Services.Library
{
    public interface IDeviceData
    {
        ApiDevice ApiDevice { get; }

        IEnumerable<ApiSensor> ApiSensors { get; }

        IEnumerable<ApiActuator> ApiActuators { get; }
    }
}
