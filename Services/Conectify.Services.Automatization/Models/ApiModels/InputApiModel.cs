using Conectify.Services.Automatization.Models.Database;

namespace Conectify.Services.Automatization.Models.ApiModels;

public class InputApiModel
{
    public Guid Id { get; set; }
    public int Index { get; set; }
    public InputTypeEnum Type { get; set; }
}
