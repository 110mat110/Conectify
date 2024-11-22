using Conectify.Services.Automatization.Models.Database;

namespace Conectify.Services.Automatization.Models.ApiModels;

public record BehaviourMenuApiModel(Guid Id, string Name, int defaultOutput, IEnumerable<Tuple<InputTypeEnum, int>> defaultInputs);
