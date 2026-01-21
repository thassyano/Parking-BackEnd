using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Estacionamento.Api.Infrastructure.Data;

public class DesignTimeDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        if (string.IsNullOrEmpty(connectionString))
        {
            // Use InMemory for design-time if no connection string
            optionsBuilder.UseInMemoryDatabase("EstacionamentoDb");
        }
        else
        {
            // Use PostgreSQL
            optionsBuilder.UseNpgsql(connectionString);
        }

        return new AppDbContext(optionsBuilder.Options);
    }
}

