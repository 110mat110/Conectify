using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Mapper;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Shared.Library.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Test.Services;

/// <summary>Translates the rule projections with the PostgreSQL provider. Only SQL text is generated (ToQueryString), no connection is opened.</summary>
public class RuleProjectionSqlTests
{
    private readonly AutomatizationDb db = new(new DbContextOptionsBuilder<AutomatizationDb>().UseNpgsql("Host=localhost;Database=never-opened").Options);
    private readonly IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<RuleProfile>()).CreateMapper();

    [Fact]
    public void RulesProjectWithInputsAndOutputs()
    {
        var sql = mapper.ProjectTo<GetRuleApiModel>(db.Set<Rule>().AsNoTracking().Include(x => x.InputConnectors).Include(x => x.OutputConnectors)).ToQueryString();

        Assert.StartsWith("SELECT", sql.TrimStart());
    }

    [Fact]
    public void ConnectionsProject()
    {
        var sql = mapper.ProjectTo<ConnectionApiModel>(db.Set<RuleConnector>()).ToQueryString();

        Assert.StartsWith("SELECT", sql.TrimStart());
    }
}
