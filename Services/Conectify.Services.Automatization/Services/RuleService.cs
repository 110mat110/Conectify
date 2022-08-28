using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.ActivityService;
using Conectify.Services.Automatization.Models.ApiModels;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Services
{
    public class RuleService
    {
        private readonly AutomatizationCache automatizationCache;
        private readonly IMapper mapper;
        private readonly ConectifyDb conectifyDb;

        public RuleService(AutomatizationCache automatizationCache, IMapper mapper, ConectifyDb conectifyDb)
        {
            this.automatizationCache = automatizationCache;
            this.mapper = mapper;
            this.conectifyDb = conectifyDb;
        }

        public async Task<Guid> AddNewRule(CreateRuleApiModel apiModel, CancellationToken cancellationToken)
        {
            var dbsModel = mapper.Map<Rule>(apiModel);

            return await automatizationCache.AddNewRule(dbsModel, cancellationToken);
        }

        public async Task<IEnumerable<GetRuleApiModel>> GetAllRules()
        {
            var rules = await conectifyDb.Set<Rule>().ToListAsync();
            return mapper.Map<IEnumerable<GetRuleApiModel>>(rules);
        }

        public async Task<bool> EditRule(Guid ruleId, string parameters)
        {
            var rule = await conectifyDb.Set<Rule>().FirstOrDefaultAsync(x => x.Id == ruleId);
            if (rule == null)
            {
                return false;
            }

            rule.ParametersJson = parameters;
            await conectifyDb.SaveChangesAsync();

            automatizationCache.Invalidate(ruleId);

            return true;
        }

        public async Task<bool> AddConnection(Guid sourceId, Guid destinationId)
        {
            var sourceRule = await automatizationCache.GetRuleById(sourceId);
            var destinationRule = await automatizationCache.GetRuleById(destinationId);

            if (sourceRule is null || destinationRule is null || sourceRule.NextRules.Contains(destinationId))
            {
                return false;
            }

            sourceRule.NextRules = sourceRule.NextRules.Append(destinationId);

            var ruleConnector = await conectifyDb.Set<RuleConnector>().FirstOrDefaultAsync(x => x.PreviousRuleId == sourceId && x.ContinuingRuleId == destinationId);
            if (ruleConnector != null)
            {
                return false;
            }

            await conectifyDb.Set<RuleConnector>().AddAsync(new RuleConnector() { ContinuingRuleId = destinationId, PreviousRuleId = sourceId });
            await conectifyDb.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveConnection(Guid sourceId, Guid destinationId)
        {
            var sourceRule = await automatizationCache.GetRuleById(sourceId);
            var destinationRule = await automatizationCache.GetRuleById(destinationId);

            if (sourceRule is null || destinationRule is null || !sourceRule.NextRules.Contains(destinationId))
            {
                return false;
            }

            var list = sourceRule.NextRules.ToList();
            list.Remove(destinationId);
            sourceRule.NextRules = list;

            var sourceRuleDb = await conectifyDb.Set<RuleConnector>().FirstOrDefaultAsync(x => x.PreviousRuleId == sourceId && x.ContinuingRuleId == destinationId);
            if (sourceRuleDb == null)
            {
                return false;
            }

            conectifyDb.Set<RuleConnector>().Remove(sourceRuleDb);
            await conectifyDb.SaveChangesAsync();

            return true;
        }
    }
}
