namespace Conectify.Services.Automatization.Models.ApiModels
{
    public record CreateRuleApiModel
    {

        public List<Guid> DestinationRules { get; set; } = new List<Guid>();

        public string Name { get; set; }

        public string Description { get; set; }

        public string Parameters { get; set; }

        public Guid RuleType { get; set; }
    }
}
