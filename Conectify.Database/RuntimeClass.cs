namespace Conectify.Database;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

[ExcludeFromCodeCoverage(Justification = "Config")]
public class DesignTimeContextFactory : IDesignTimeDbContextFactory<ConectifyDb>
{
    public ConectifyDb CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettingsDbs.json")
           .Build();

        var options = new DbContextOptionsBuilder<ConectifyDb>();
        options.UseNpgsql(
            configuration.GetConnectionString("DatabaseString"),
                sqlServerOptions => sqlServerOptions
                    .MigrationsAssembly("Conectify.Database"));

        return new ConectifyDb(options.Options);
    }
}
