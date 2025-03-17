using Conectify.Services.Automatization.Models.Database;

namespace Conectify.Services.Automatization.Models.ApiModels;

public record BehaviourMenuApiModel(Guid Id, string Name, MinMaxDef Outputs, IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs);

public record MinMaxDef(int Min, int Def, int Max);
