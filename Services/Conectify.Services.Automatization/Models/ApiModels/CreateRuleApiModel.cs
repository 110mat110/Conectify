namespace Conectify.Services.Automatization.Models.ApiModels
{
    public record CreateRuleApiModel
    {
        public List<Guid> DestinationRules { get; set; } = new List<Guid>();

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Parameters { get; set; } = string.Empty;

        public Guid RuleType { get; set; }
    }
}
