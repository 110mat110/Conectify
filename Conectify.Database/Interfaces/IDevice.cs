namespace Conectify.Database.Interfaces;
public interface IDevice
{
    bool IsKnown { get; set; }
    string Name { get; set; }
}
