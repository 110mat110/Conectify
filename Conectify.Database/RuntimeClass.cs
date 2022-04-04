namespace Conectify.Database;

using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class DesignTimeContextFactory : IDesignTimeDbContextFactory<ConectifyDb>
{
    public ConectifyDb CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.Development.json")
           .Build();

        var options = new DbContextOptionsBuilder<ConectifyDb>();
        options.UseNpgsql(
            configuration.GetConnectionString("DatabaseString"),
                sqlServerOptions => sqlServerOptions
                    .MigrationsAssembly("Conectify.Database"));

        return new ConectifyDb(options.Options);
    }
}
