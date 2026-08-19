using Conectify.Services.Automatization.Models.Database;

namespace Conectify.Services.Automatization.Models.ApiModels;

/// <summary>
/// <paramref name="IsInternal"/> marks behaviours that belong to another feature - function ports are
/// created from the function itself, so the plain "new rule" menu leaves them out.
/// </summary>
public record BehaviourMenuApiModel(Guid Id, string Name, MinMaxDef Outputs, IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs, bool IsInternal = false);

public record MinMaxDef(int Min, int Def, int Max);
