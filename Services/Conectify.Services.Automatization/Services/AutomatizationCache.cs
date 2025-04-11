using AutoMapper;
using Conectify.Database;
using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationCache
{
    Task<Guid> AddNewRule(Rule rule, CancellationToken cancellationToken);
    Task<RuleDTO?> GetRuleByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<RuleDTO>> GetRulesByTypeIdAsync(Guid ruleTypeId);
    Task<IEnumerable<RuleDTO>> GetRulesForSourceAsync(Guid sourceId, CancellationToken ct = default);
    Task Reload(Guid id, CancellationToken ct = default);

    Task<IEnumerable<RuleDTO>> GetAllRulesAsync();
    Task<OutputPointDTO?> GetOutputPointById(Guid sourceId);
    Task<InputPointDTO?> GetInputPointById(Guid destinationId);

    bool ConnectionExist(Guid sourceId, Guid destinationId);
    Task ReloadConnections();

    Task<IEnumerable<InputPointDTO>> GetNextInputs(Guid outputId);
    Task<IEnumerable<OutputPointDTO>> GetPreviousOutputs(Guid inputId);
}

public class AutomatizationCache : IAutomatizationCache
{
    private readonly IServiceProvider services;
    private readonly IMapper mapper;
    private IDictionary<Guid, RuleDTO> cache = new Dictionary<Guid, RuleDTO>();
    private List<Tuple<Guid, Guid>> connections = [];
    private DateTime lastReload;
    private readonly TimeSpan cacheLongevity = new(0, 10, 0);

    private static bool reloading;
    public AutomatizationCache(IServiceProvider services, IMapper mapper)
    {
        this.services = services;
        this.mapper = mapper;
        _ = Reload().Result;
    }

    public async Task<RuleDTO?> GetRuleByIdAsync(Guid id, CancellationToken ct = default)
    {
        await ReloadIfNeeded();

        if (cache.TryGetValue(id, out RuleDTO? rule))
        {
            return rule;
        }

        await Reload(id, ct);

        return cache.TryGetValue(id, out RuleDTO? value) ? value : null;
    }

    public async Task<IEnumerable<RuleDTO>> GetRulesForSourceAsync(Guid sourceId, CancellationToken ct = default)
    {
        await ReloadIfNeeded();
        return cache.Where(x => x.Value.SourceSensorId == sourceId).Select(x => x.Value);
    }

    public async Task<IEnumerable<RuleDTO>> GetRulesByTypeIdAsync(Guid ruleTypeId)
    {
        await ReloadIfNeeded();
        return cache.Where(x => x.Value.RuleTypeId == ruleTypeId).Select(x => x.Value);
    }

    public async Task<Guid> AddNewRule(Rule rule, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var automatizaionDb = scope.ServiceProvider.GetRequiredService<AutomatizationDb>();
        await automatizaionDb.Set<Rule>().AddAsync(rule, cancellationToken);
        await automatizaionDb.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<RuleDTO>(rule);
        await dto.InitializeAsync(services, null);

        cache.TryAdd(dto.Id, dto);
        return rule.Id;
    }

    private async Task ReloadIfNeeded()
    {
        if (DateTime.UtcNow.Subtract(lastReload).CompareTo(cacheLongevity) > 0)
        {
           await Reload();
        }
    }

    public async Task Reload(Guid id, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var automatizationDb = scope.ServiceProvider.GetRequiredService<AutomatizationDb>();
        var rule = await automatizationDb.Set<Rule>().AsNoTracking().Include(x => x.OutputConnectors).Include(x => x.InputConnectors).FirstOrDefaultAsync(x => x.Id == id, ct);

        if(rule is null)
        {
            cache.Remove(id);
            return;
        }

        var dto = mapper.Map<RuleDTO>(rule);
        var outputs = new List<OutputPointDTO>();
        foreach (var output in rule.OutputConnectors)
        {
            outputs.Add(new OutputPointDTO(output.Id, services));
        }
        foreach (var input in dto.Inputs)
        {
            input.Rule = dto;
        }
        dto.Outputs = outputs;
        cache.TryGetValue(id, out var oldDto);
        await dto.InitializeAsync(services, oldDto);

        cache.Remove(id);
        cache.Add(dto.Id, dto);
    }

    private async Task<bool> Reload()
    {
        if(reloading) return false;
        reloading = true;
        await ReloadRules();
        await ReloadConnections();

        reloading = false;

        return true;
    }

    public async Task<IEnumerable<RuleDTO>> GetAllRulesAsync()
    {
        await ReloadIfNeeded();
        return [.. cache.Values];
    }

    public async Task<OutputPointDTO?> GetOutputPointById(Guid sourceId)
    {
        await ReloadIfNeeded();
        var ids = cache.Values.SelectMany(x => x.Outputs.Select(o => o.Id)).ToList();
        return cache.Values.SelectMany(x => x.Outputs).FirstOrDefault(x => x.Id == sourceId);
    }

    public async Task<InputPointDTO?> GetInputPointById(Guid destinationId)
    {
        await ReloadIfNeeded();
        return cache.SelectMany(x => x.Value.Inputs).FirstOrDefault(x => x.Id == destinationId);
    }

    public async Task<IEnumerable<InputPointDTO>> GetNextInputs(Guid outputId)
    {
        await ReloadIfNeeded();

        var inputIds = connections.Where(x => x.Item1 == outputId).Select(x => x.Item2).ToList();

       return cache.SelectMany(x => x.Value.Inputs).Where(x => inputIds.Contains(x.Id));
    }

    public async Task<IEnumerable<OutputPointDTO>> GetPreviousOutputs(Guid inputId)
    {
        await ReloadIfNeeded();

        var outputIds = connections.Where(x => x.Item2 == inputId).Select(x => x.Item1).ToList();

        return cache.SelectMany(x => x.Value.Outputs).Where(x => outputIds.Contains(x.Id));

    }

    public bool ConnectionExist(Guid sourceId, Guid destinationId)
    {
        return connections.Any(x => x.Item1 == sourceId && x.Item2 == destinationId);
    }

    public async Task ReloadConnections()
    {
        connections.Clear();
        using var scope = services.CreateScope();
        var automatizationDb = scope.ServiceProvider.GetRequiredService<AutomatizationDb>();
        connections = await automatizationDb.Set<RuleConnector>().AsNoTracking().Select(x => new Tuple<Guid, Guid>(x.SourceRuleId, x.TargetRuleId)).ToListAsync();
    }

    private async Task ReloadRules()
    {
        var scope = services.CreateScope();
        var automatizationDb = scope.ServiceProvider.GetRequiredService<AutomatizationDb>();
        var dbrules = await automatizationDb.Set<Rule>().AsNoTracking().Include(x => x.OutputConnectors).Include(x => x.InputConnectors).ToListAsync();

        var dtos = mapper.Map<IEnumerable<RuleDTO>>(dbrules);
        foreach (var dto in dtos)
        {
            var outputs = new List<OutputPointDTO>();
            var rule = dbrules.First(x => x.Id == dto.Id);
            foreach (var output in rule.OutputConnectors)
            {
                outputs.Add(new OutputPointDTO(output.Id, services));
            }
            foreach (var input in dto.Inputs)
            {
                input.Rule = dto;
            }
            dto.Outputs = outputs;
        }

        var tempcache = dtos.ToDictionary(x => x.Id);

        foreach (var d in tempcache.Values)
        {
            var exist = cache.TryGetValue(d.Id, out var oldDto);
            await d.InitializeAsync(services, oldDto);
        }
        cache.Clear();
        cache = tempcache;
        lastReload = DateTime.UtcNow;
    }
}
