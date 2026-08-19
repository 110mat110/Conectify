using AutoMapper;
using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Rules;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Services;

/// <summary>
/// Functions group rules together and can be nested. A function borrows the plain rule machinery for
/// its interface: every port is a rule inside the function, so a connection reaching across the
/// border is still an ordinary output to input connection and the engine needs to know nothing.
/// </summary>
public class FunctionService(IAutomatizationCache automatizationCache, IMapper mapper, AutomatizationDb database, RuleService ruleService, IServiceProvider services, ILogger<FunctionService> logger)
{
    /// <summary>Distance between two ports stacked on the board of the function.</summary>
    private const int PortSpacing = 170;

    private static readonly Guid InputPortBehaviourId = FunctionPortBehaviourIds.Input;
    private static readonly Guid OutputPortBehaviourId = FunctionPortBehaviourIds.Output;

    public async Task<IEnumerable<GetFunctionApiModel>> GetAllFunctions(CancellationToken ct = default)
    {
        var functions = await database.Functions.AsNoTracking().ToListAsync(ct);
        var portRules = await database.Rules.AsNoTracking()
            .Where(x => x.FunctionId != null && (x.RuleType == InputPortBehaviourId || x.RuleType == OutputPortBehaviourId))
            .Include(x => x.InputConnectors)
            .Include(x => x.OutputConnectors)
            .ToListAsync(ct);

        return functions.Select(function => new GetFunctionApiModel()
        {
            Id = function.Id,
            Name = function.Name,
            Description = function.Description,
            ParentFunctionId = function.ParentFunctionId,
            X = function.X,
            Y = function.Y,
            InputPorts = MapPorts(portRules, function.Id, FunctionPortDirection.Input),
            OutputPorts = MapPorts(portRules, function.Id, FunctionPortDirection.Output),
        }).ToList();
    }

    public async Task<GetFunctionApiModel> AddFunction(CreateFunctionApiModel apiModel, CancellationToken ct = default)
    {
        logger.LogInformation("AddFunction: name={Name} parent={ParentId}", apiModel.Name, apiModel.ParentFunctionId);

        if (apiModel.ParentFunctionId is not null && !await database.Functions.AnyAsync(x => x.Id == apiModel.ParentFunctionId, ct))
        {
            throw new ArgumentException("Parent function does not exist!");
        }

        var function = new Function()
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(apiModel.Name) ? "New function" : apiModel.Name.Trim(),
            Description = string.Empty,
            ParentFunctionId = apiModel.ParentFunctionId,
        };

        await database.Functions.AddAsync(function, ct);
        await database.SaveChangesAsync(ct);

        return new GetFunctionApiModel()
        {
            Id = function.Id,
            Name = function.Name,
            Description = function.Description,
            ParentFunctionId = function.ParentFunctionId,
            X = function.X,
            Y = function.Y,
        };
    }

    public async Task<bool> EditFunction(Guid id, EditFunctionApiModel apiModel, CancellationToken ct = default)
    {
        var function = await database.Functions.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (function is null)
        {
            logger.LogWarning("EditFunction: function {FunctionId} not found", id);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(apiModel.Name))
        {
            function.Name = apiModel.Name.Trim();
        }

        function.Description = apiModel.Description;
        function.X = apiModel.X;
        function.Y = apiModel.Y;

        await database.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>Deletes the function with everything it holds - nested functions and their rules.</summary>
    public async Task<bool> RemoveFunction(Guid id, CancellationToken ct = default)
    {
        var function = await database.Functions.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (function is null)
        {
            logger.LogWarning("RemoveFunction: function {FunctionId} not found", id);
            return false;
        }

        var functionIds = await CollectWithDescendants(id, ct);
        var ruleIds = await database.Rules.Where(x => x.FunctionId != null && functionIds.Contains(x.FunctionId!.Value))
            .Select(x => x.Id)
            .ToListAsync(ct);

        logger.LogInformation("RemoveFunction: deleting {FunctionCount} function(s) and {RuleCount} rule(s)", functionIds.Count, ruleIds.Count);

        await RemovePointsAndConnections(ruleIds, ct);

        // The rule itself goes through the rule service, so that the cache is refreshed the same way
        // as for a rule deleted on its own.
        foreach (var ruleId in ruleIds)
        {
            await ruleService.Remove(ruleId, ct);
        }

        var functions = await database.Functions.Where(x => functionIds.Contains(x.Id)).ToListAsync(ct);
        database.Functions.RemoveRange(functions);
        await database.SaveChangesAsync(ct);

        await automatizationCache.ReloadConnections();

        return true;
    }

    /// <summary>Creates a port and the rule behind it on the board of the function.</summary>
    public async Task<GetRuleApiModel> AddPort(Guid functionId, AddFunctionPortApiModel apiModel, CancellationToken ct = default)
    {
        if (!await database.Functions.AnyAsync(x => x.Id == functionId, ct))
        {
            throw new ArgumentException("Function does not exist!");
        }

        var behaviour = apiModel.Direction == FunctionPortDirection.Input
            ? (FunctionPortRuleBehaviour)new FunctionInputRuleBehaviour(services)
            : new FunctionOutputRuleBehaviour(services);

        var alreadyThere = await database.Rules.CountAsync(x => x.FunctionId == functionId && x.RuleType == behaviour.GetId(), ct);

        var rule = new Rule()
        {
            Id = Guid.NewGuid(),
            FunctionId = functionId,
            RuleType = behaviour.GetId(),
            ParametersJson = JsonConvert.SerializeObject(new FunctionPortRuleBehaviour.FunctionPortOptions()
            {
                Name = string.IsNullOrWhiteSpace(apiModel.Name) ? $"Port {alreadyThere + 1}" : apiModel.Name.Trim(),
            }),
            // Ports are laid out in a column so that a new one never lands on top of another, and the
            // position doubles as the order of the pins drawn on the parent board.
            X = apiModel.Direction == FunctionPortDirection.Input ? 150 : 750,
            Y = 150 + (alreadyThere * PortSpacing),
            InputConnectors = new List<InputPoint>
            {
                new() { Id = Guid.NewGuid(), Index = 0, Type = apiModel.Type },
            },
            OutputConnectors = new List<OutputPoint>
            {
                new() { Id = Guid.NewGuid(), Index = 0 },
            },
        };

        await behaviour.SetParameters(rule, ct);
        await automatizationCache.AddNewRule(rule, ct);

        logger.LogInformation("AddPort: function={FunctionId} direction={Direction} rule={RuleId}", functionId, apiModel.Direction, rule.Id);

        return mapper.Map<GetRuleApiModel>(rule);
    }

    /// <summary>Moves a rule into a function, or back onto the top level board when null is given.</summary>
    public async Task<bool> MoveRule(Guid ruleId, Guid? functionId, CancellationToken ct = default)
    {
        var rule = await database.Rules.FirstOrDefaultAsync(x => x.Id == ruleId, ct);

        if (rule is null)
        {
            logger.LogWarning("MoveRule: rule {RuleId} not found", ruleId);
            return false;
        }

        if (functionId is not null && !await database.Functions.AnyAsync(x => x.Id == functionId, ct))
        {
            return false;
        }

        rule.FunctionId = functionId;
        await database.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>Moves a whole function into another one. A function can never end up inside itself.</summary>
    public async Task<bool> MoveFunction(Guid functionId, Guid? parentFunctionId, CancellationToken ct = default)
    {
        var function = await database.Functions.FirstOrDefaultAsync(x => x.Id == functionId, ct);

        if (function is null)
        {
            return false;
        }

        if (parentFunctionId is not null)
        {
            if (!await database.Functions.AnyAsync(x => x.Id == parentFunctionId, ct))
            {
                return false;
            }

            var descendants = await CollectWithDescendants(functionId, ct);

            if (descendants.Contains(parentFunctionId.Value))
            {
                logger.LogWarning("MoveFunction: {FunctionId} cannot be moved into itself", functionId);
                return false;
            }
        }

        function.ParentFunctionId = parentFunctionId;
        await database.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>
    /// Takes the points of the given rules down together with every connection reaching them. A rule
    /// inside a function is often wired to the board outside of it, and those connections have to go
    /// with it instead of pointing at a socket that no longer exists.
    /// </summary>
    private async Task RemovePointsAndConnections(List<Guid> ruleIds, CancellationToken ct)
    {
        var inputs = await database.InputConnectors.Where(x => ruleIds.Contains(x.RuleId)).ToListAsync(ct);
        var outputs = await database.OutputConnectors.Where(x => ruleIds.Contains(x.RuleId)).ToListAsync(ct);

        var inputIds = inputs.Select(x => x.Id).ToList();
        var outputIds = outputs.Select(x => x.Id).ToList();

        var connectors = await database.Set<RuleConnector>()
            .Where(x => inputIds.Contains(x.TargetRuleId) || outputIds.Contains(x.SourceRuleId))
            .ToListAsync(ct);

        database.Set<RuleConnector>().RemoveRange(connectors);
        database.InputConnectors.RemoveRange(inputs);
        database.OutputConnectors.RemoveRange(outputs);

        await database.SaveChangesAsync(ct);
    }

    private async Task<List<Guid>> CollectWithDescendants(Guid id, CancellationToken ct)
    {
        var all = await database.Functions.AsNoTracking().Select(x => new { x.Id, x.ParentFunctionId }).ToListAsync(ct);
        var collected = new List<Guid>() { id };
        var frontier = new Queue<Guid>();
        frontier.Enqueue(id);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            foreach (var child in all.Where(x => x.ParentFunctionId == current && !collected.Contains(x.Id)))
            {
                collected.Add(child.Id);
                frontier.Enqueue(child.Id);
            }
        }

        return collected;
    }

    private static IEnumerable<FunctionPortApiModel> MapPorts(IEnumerable<Rule> portRules, Guid functionId, FunctionPortDirection direction)
    {
        var behaviourId = direction == FunctionPortDirection.Input ? InputPortBehaviourId : OutputPortBehaviourId;

        return portRules
            .Where(x => x.FunctionId == functionId && x.RuleType == behaviourId)
            .OrderBy(x => x.Y).ThenBy(x => x.X).ThenBy(x => x.Id)
            .Select((rule, index) => new FunctionPortApiModel()
            {
                RuleId = rule.Id,
                // The parent board plugs into the input of an input port and reads the output of an
                // output port, so each direction reports the point that faces outwards.
                PointId = direction == FunctionPortDirection.Input
                    ? rule.InputConnectors.OrderBy(p => p.Index).Select(p => p.Id).FirstOrDefault()
                    : rule.OutputConnectors.OrderBy(p => p.Index).Select(p => p.Id).FirstOrDefault(),
                Name = rule.Name,
                Type = rule.InputConnectors.OrderBy(p => p.Index).Select(p => p.Type).FirstOrDefault(),
                Index = index,
            })
            .Where(x => x.PointId != Guid.Empty)
            .ToList();
    }
}
