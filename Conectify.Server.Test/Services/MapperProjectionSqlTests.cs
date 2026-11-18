using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Server.Test.Services;

/// <summary>
/// The in-memory provider accepts almost any projection, so these translate the queries with the PostgreSQL provider.
/// Only SQL text is generated (ToQueryString), no connection is opened.
/// </summary>
public class MapperProjectionSqlTests
{
    private readonly ConectifyDb db;
    private readonly IMapper mapper;

    public MapperProjectionSqlTests()
    {
        db = new ConectifyDb(new DbContextOptionsBuilder<ConectifyDb>().UseNpgsql("Host=localhost;Database=never-opened").Options);
        mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DeviceProfile>();
            cfg.AddProfile<MetadataProfile>();
            cfg.AddProfile<EventProfile>();
        }).CreateMapper();
    }

    [Fact]
    public void DevicesProjectWithMetadataNames() => AssertSql(mapper.ProjectTo<ApiDevice>(db.Devices.AsNoTracking()).ToQueryString(), "Name");

    [Fact]
    public void SensorsProjectWithMetadataNames() => AssertSql(mapper.ProjectTo<ApiSensor>(db.Sensors.AsNoTracking()).ToQueryString(), "Name");

    [Fact]
    public void ActuatorsProjectWithMetadataNames() => AssertSql(mapper.ProjectTo<ApiActuator>(db.Actuators.AsNoTracking()).ToQueryString(), "Name");

    [Fact]
    public void DeviceMetadataProjectsMetadataName() => AssertSql(mapper.ProjectTo<ApiMetadata>(db.Set<MetadataConnector<Device>>().AsNoTracking()).ToQueryString(), "Name");

    [Fact]
    public void BasicMetadataProjects() => AssertSql(mapper.ProjectTo<ApiBasicMetadata>(db.Set<Metadata>()).ToQueryString(), "Code");

    [Fact]
    public void EventsProject() => AssertSql(mapper.ProjectTo<ApiEvent>(db.Set<Event>().AsNoTracking().OrderByDescending(x => x.TimeCreated)).ToQueryString(), "TimeCreated");

    private static void AssertSql(string sql, string expectedColumn)
    {
        Assert.StartsWith("SELECT", sql.TrimStart());
        Assert.Contains(expectedColumn, sql);
    }
}
