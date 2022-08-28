using AutoMapper;
using Conectify.Database.Models.ActivityService;
using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Mapper
{
    public class RuleProfile : Profile
    {
        public RuleProfile()
        {
            this.CreateMap<Rule, RuleDTO>()
                .ForMember(x => x.Values, opt => opt.Ignore())
                .ForMember(x => x.NextRules, opt => opt.MapFrom(x => x.ContinuingRules.Select(x => x.ContinuingRuleId)));


            this.CreateMap<RuleDTO, Rule>();
        }


    }
}
