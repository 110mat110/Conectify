using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.ActivityService;
using Conectify.Services.Automatization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Conectify.Services.Automatization.Services;

public class AutomatizationCache
{
    private readonly IServiceProvider services;
    private readonly IMapper mapper;
    private IDictionary<Guid, RuleDTO> cache = new Dictionary<Guid, RuleDTO>();
    private DateTime lastReload;
    private TimeSpan cacheLongevity = new TimeSpan(0, 10, 0);
    public AutomatizationCache(IServiceProvider services, IMapper mapper)
    {
        this.services = services;
        this.mapper = mapper;
        Reload();
    }

    public async Task<RuleDTO?> GetRuleByIdAsync(Guid id)
    {
        if (cache.ContainsKey(id))
        {
            return cache[id];
        }

        await Reload(id);

        return cache.ContainsKey(id) ? cache[id] : null;
    }

    public IEnumerable<RuleDTO> GetRulesForSource(Guid sourceId, CancellationToken ct = default)
    {
        ReloadIfNeeded();
        return cache.Where(x => x.Value.SourceSensorId == sourceId).Select(x => x.Value);
    }

    public IEnumerable<RuleDTO> GetRulesByTypeId(Guid ruleTypeId)
    {
        ReloadIfNeeded();
        return cache.Where(x => x.Value.RuleTypeId == ruleTypeId).Select(x => x.Value);
    }


    public IEnumerable<RuleDTO> GetNextRules(RuleDTO ruleDTO, CancellationToken ct = default)
    {
        ReloadIfNeeded();
        return cache.Where(x => ruleDTO.NextRules.Contains(x.Value.Id)).Select(x => x.Value);
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

    private void ReloadIfNeeded()
    {
        if(DateTime.UtcNow.Subtract(lastReload).CompareTo(cacheLongevity) > 0)
        {
            Reload();
        }
    }

    public async Task Reload(Guid id, CancellationToken ct = default)
    {
        cache.Remove(id);

        using var scope = services.CreateScope();
        var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        var rule = await conectifyDb.Set<Rule>().FirstAsync(x => x.Id == id, ct);

        var dto = mapper.Map<RuleDTO>(rule);

        cache.Add(dto.Id, dto);
    }

    //public async Task Reload(CancellationToken ct = default)
    //{
    //    cache.Clear();
    //    using var scope = services.CreateScope();
    //    var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
    //    var dbrules = await conectifyDb.Set<Rule>().Include(x => x.ContinuingRules).ToListAsync(ct);

    //    var dtos = mapper.Map<IEnumerable<RuleDTO>>(dbrules);
    //    cache = dtos.ToDictionary(x => x.Id);
    //    lastReload = DateTime.UtcNow;
    //}

    private void  Reload()
    {
        cache.Clear();
        using var scope = services.CreateScope();
        var conectifyDb = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        var dbrules = conectifyDb.Set<Rule>().Include(x => x.ContinuingRules).ToList();

        var dtos = mapper.Map<IEnumerable<RuleDTO>>(dbrules);
        cache = dtos.ToDictionary(x => x.Id);
        lastReload = DateTime.UtcNow;
    }
}
