﻿namespace Conectify.Database;

using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Database.Models.ActivityService;
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

    //Metadata
    public DbSet<Metadata> Metadatas { get; set; } = null!;
    public DbSet<MetadataConnector<Actuator>> ActuatorMetadatas { get; set; } = null!;
    public DbSet<MetadataConnector<Device>> DeviceMetadata { get; set; } = null!;
    public DbSet<MetadataConnector<Sensor>> SensorMetadata { get; set; } = null!;


    //Rules
    public DbSet<Rule> Rules { get; set; } = null!;

    public async Task<T> AddOrUpdateAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity
    {
        if (await this.Set<T>().AnyAsync(d => d.Id == entity.Id, ct))
        {
            this.Update(entity);
        }
        else
        {
            await this.AddAsync(entity, ct);
        }

        return entity;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetadataConnector<Actuator>>().HasKey(u => new
        {
            u.DeviceId,
            u.MetadataId
        });

        modelBuilder.Entity<MetadataConnector<Sensor>>().HasKey(u => new
        {
            u.DeviceId,
            u.MetadataId
        });

        modelBuilder.Entity<MetadataConnector<Device>>().HasKey(u => new
        {
            u.DeviceId,
            u.MetadataId
        });

        modelBuilder.Entity<RuleConnector>().HasKey(u => new
        {
            u.PreviousRuleId,
            u.ContinuingRuleId,
        });

        modelBuilder.Entity<RuleConnector>()
            .HasOne(bc => bc.ContinuingRule)
            .WithMany(b => b.PreviousRules)
            .HasForeignKey(bc => bc.ContinuingRuleId);
        modelBuilder.Entity<RuleConnector>()
            .HasOne(bc => bc.PreviousRule)
            .WithMany(c => c.ContinuingRules)
            .HasForeignKey(bc => bc.PreviousRuleId);

        modelBuilder.Entity<RuleParameter>().HasKey(u => new
        {
            u.SourceRuleId,
            u.TargetRuleId,
        });

        modelBuilder.Entity<RuleParameter>()
            .HasOne(bc => bc.SourceRule)
            .WithMany(b => b.TargetParameters)
            .HasForeignKey(bc => bc.SourceRuleId);
        modelBuilder.Entity<RuleParameter>()
            .HasOne(bc => bc.TargetRule)
            .WithMany(c => c.SourceParameters)
            .HasForeignKey(bc => bc.TargetRuleId);

        modelBuilder.Entity<Preference>()
            .HasOne(bc => bc.Subscriber)
            .WithMany(b => b.Preferences)
            .HasForeignKey(bc => bc.SubscriberId);
    }

}
