using Conectify.Services.Automatization.Models;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules
{
    public class SetValueRuleBehaviour : IRuleBehaviour
    {
        public AutomatisationValue Execute(IEnumerable<AutomatisationValue> automationValues, RuleDTO ruleDTO)
        {
            var value  = JsonConvert.DeserializeObject<SetValueOptions>(ruleDTO.ParametersJson);

            if(value == null)
            {
                return automationValues.First();
            }

            return new AutomatisationValue()
            {
                Name = "Static value",
                NumericValue = value.NumericValue,
                StringValue = value.StringValue,
                Unit = value.Unit,
                TimeCreated = automationValues.First().TimeCreated,
                SourceId = automationValues.First().SourceId,
            };
        }

        public Guid GetId()
        {
            return Guid.Parse("8c173ffc-7243-4675-9a0d-28c2ce19a18f");
        }

        private record SetValueOptions
        {
            public string StringValue = string.Empty;
            public float NumericValue = 0;
            public string Unit = string.Empty;
        }
    }
}
