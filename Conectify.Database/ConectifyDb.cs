namespace Conectify.Database;

using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Microsoft.EntityFrameworkCore;

public class ConectifyDb : DbContext
{
    public ConectifyDb(DbContextOptions<ConectifyDb> options)
    : base(options)
    {
    }

    //Values
    public DbSet<Value> Values { get; set; } = null!;
    //Actions
    public DbSet<Action> Actions { get; set; } = null!;
    public DbSet<ActionResponse> ActionResponses { get; set; } = null!;
    //Commands
    public DbSet<Command> Commands { get; set; } = null!;
    public DbSet<CommandResponse> CommandResponses { get; set; } = null!;

    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<Sensor> Sensors { get; set; } = null!;
    public DbSet<Actuator> Actuators { get; set; } = null!;
    public DbSet<Metadata> UniversalMetadatas { get; set; } = null!;

    public async Task<T> AddOrUpdateAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity
    {
        var dbEntity = this.Set<T>().AsNoTracking().FirstOrDefault(d => d.Id == entity.Id);

        if (dbEntity is null)
        {
            await this.AddAsync(entity, ct);
        }
        else
        {
            this.Update(entity);
        }

        return entity;
    }
}
