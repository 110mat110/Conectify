﻿namespace Conectify.Database;

using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Database.Models.Automatization;
using Conectify.Database.Models.Dashboard;
using Conectify.Database.Models.Shelly;
using Conectify.Database.Models.Updates;
using Conectify.Database.Models.Values;
using Microsoft.EntityFrameworkCore;

public class ConectifyDb(DbContextOptions<ConectifyDb> options) : DbContext(options)
{

    //Values
    public DbSet<Event> Events { get; set; } = null!;

    //Devices
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

    //Dashboard
    public DbSet<Dashboard> Dashboards { get; set; }
    public DbSet<User> Users { get; set; }

    public DbSet<DashboardDevice> DashboardsDevice { get; set; }

    //Automatic updated
    public DbSet<DeviceVersion> DeviceVersions { get; set; }
    public DbSet<Software> Softwares { get; set; }
    public DbSet<SoftwareVersion> SoftwareVersions { get; set; }

    //Shelly
    public DbSet<Shelly> Shellys { get; set; }

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

        modelBuilder.Entity<DeviceVersion>()
            .HasOne(bc => bc.Device)
            .WithMany(bc => bc.DeviceVersions)
            .HasForeignKey(bc => bc.DeviceId);
        modelBuilder.Entity<DeviceVersion>()
            .HasKey(x => x.DeviceId);

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
