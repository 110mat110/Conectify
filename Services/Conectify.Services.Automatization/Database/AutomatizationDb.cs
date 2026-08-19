using Conectify.Services.Automatization.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Automatization.Database;

public class AutomatizationDb(DbContextOptions<AutomatizationDb> options) : DbContext(options)
{
    //Rules
    public DbSet<Rule> Rules { get; set; } = null!;

    public DbSet<OutputPoint> OutputConnectors { get; set; } = null!;

    public DbSet<InputPoint> InputConnectors { get; set; } = null!;

    public DbSet<Function> Functions { get; set; } = null!;



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RuleConnector>().HasKey(u => new
        {
            u.SourceRuleId,
            u.TargetRuleId,
        });

        // A function is deleted with everything it holds - nested functions and rules alike.
        modelBuilder.Entity<Function>()
            .HasOne(x => x.ParentFunction)
            .WithMany(x => x.Functions)
            .HasForeignKey(x => x.ParentFunctionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Function>()
            .HasMany(x => x.Rules)
            .WithOne(x => x.Function)
            .HasForeignKey(x => x.FunctionId)
            .OnDelete(DeleteBehavior.Cascade);

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
