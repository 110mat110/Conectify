using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.ActivityService;
using Conectify.Services.Automatization.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Services;

public class AutomatizationCache
{
    private readonly IServiceProvider services;
    private readonly IMapper mapper;
    private readonly IDictionary<Guid, RuleDTO> cache;
    public AutomatizationCache(IServiceProvider services, IMapper mapper)
    {
        this.services = services;
        this.mapper = mapper;
        cache = new Dictionary<Guid, RuleDTO>();
    }

    public async Task<RuleDTO?> GetRuleById(Guid id)
    {
        if (cache.ContainsKey(id))
        {
            return cache[id];
        }

        using var scope = services.CreateScope();
        var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        //Todo guess what to do with exceptions later
        var rule = await conectifyDb.Set<Rule>().FirstAsync(x => x.Id == id);

        var dto = mapper.Map<RuleDTO>(rule);

        cache.Add(id, dto);

        return dto;
    }

    public async Task<IEnumerable<RuleDTO>> GetRulesForSource(Guid sourceId)
    {
        using var scope = services.CreateScope();
        var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        var rules = await conectifyDb.Set<Rule>().Where(x => x.SourceSensorId == sourceId).ToListAsync();
        var dtos = mapper.Map<IEnumerable<RuleDTO>>(rules);

        foreach (var rule in dtos)
        {
            cache.TryAdd(rule.Id, rule);
        }
        var selectedKeys = dtos.Select(x => x.Id);
        return cache.Where(x => selectedKeys.Contains(x.Key)).Select(x => x.Value);
    }

    public async Task<IEnumerable<RuleDTO>> GetRulesByTypeId(Guid ruleTypeId)
    {
        using var scope = services.CreateScope();
        var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        var rules = await conectifyDb.Set<Rule>().Where(x => x.RuleType == ruleTypeId).ToListAsync();
        var dtos = mapper.Map<IEnumerable<RuleDTO>>(rules);

        foreach (var rule in dtos)
        {
            cache.TryAdd(rule.Id, rule);
        }
        var selectedKeys = dtos.Select(x => x.Id);
        return cache.Where(x => selectedKeys.Contains(x.Key)).Select(x => x.Value);

    }


    public async Task<IEnumerable<RuleDTO>> GetNextRules(RuleDTO ruleDTO)
    {
        using var scope = services.CreateScope();
        var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        var rules = await conectifyDb.Set<Rule>().Where(x => ruleDTO.NextRules.Contains(x.Id)).ToListAsync();
        var dtos = mapper.Map<IEnumerable<RuleDTO>>(rules);

        foreach (var rule in dtos)
        {
            cache.TryAdd(rule.Id, rule);
        }
        var selectedKeys = dtos.Select(x => x.Id);
        return cache.Where(x => selectedKeys.Contains(x.Key)).Select(x => x.Value);
    }

    public async Task<Guid> AddNewRule(Rule rule, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        await conectifyDb.Set<Rule>().AddAsync(rule, cancellationToken);
        await conectifyDb.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<RuleDTO>(rule);

        cache.TryAdd(dto.Id, dto);
        return rule.Id;
    }

    public void Invalidate(Guid id)
    {
        cache.Remove(id);
    }
}
