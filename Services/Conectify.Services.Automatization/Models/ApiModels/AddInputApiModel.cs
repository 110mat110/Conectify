using Conectify.Services.Automatization.Models.Database;
namespace Conectify.Services.Automatization.Models.ApiModels;

public class AddInputApiModel
{
    public Guid RuleId { get; set; }
    public InputTypeEnum InputType { get; set; }

    public int Index { get; set; }
}
