namespace Conectify.Services.Automatization.Models.ApiModels;

/// <summary><paramref name="FunctionId"/> keeps the created input rule inside the function it was added from.</summary>
public record AddActuatorApiModel(string ActuatorName, Guid? FunctionId = null);
