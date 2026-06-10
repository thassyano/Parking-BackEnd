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
            optionsBuilder.UseInMemoryDatabase("EstacionamentoDb");
        }
        else
        {
            string finalConnectionString = connectionString;

            if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? userInfo[0] : "postgres";
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

                finalConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            }

            optionsBuilder.UseNpgsql(finalConnectionString);
        }

        return new AppDbContext(optionsBuilder.Options);
    }
}
