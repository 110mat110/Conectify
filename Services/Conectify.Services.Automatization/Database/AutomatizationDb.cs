using Conectify.Services.Automatization.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Database;

public class AutomatizationDb(DbContextOptions<AutomatizationDb> options) : DbContext(options)
{
    //Rules
    public DbSet<Rule> Rules { get; set; } = null!;

    public DbSet<OutputPoint> OutputConnectors { get; set; } = null!;

    public DbSet<InputPoint> InputConnectors { get; set; } = null!;



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RuleConnector>().HasKey(u => new
        {
            u.SourceRuleId,
            u.TargetRuleId,
        });

        modelBuilder.Entity<RuleConnector>()
            .HasOne(bc => bc.SourceRule)
            .WithMany(b => b.ContinousRules)
            .HasForeignKey(bc => bc.SourceRuleId);
        modelBuilder.Entity<RuleConnector>()
            .HasOne(bc => bc.TargetRule)
            .WithMany(c => c.PreviousRules)
            .HasForeignKey(bc => bc.TargetRuleId);
    }
}
