using BrokerOS.Application.Abstractions;
using BrokerOS.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BrokerOS.Infrastructure.Persistence;

public sealed class BrokerOsDbContextFactory : IDesignTimeDbContextFactory<BrokerOsDbContext>
{
    public BrokerOsDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiPath = FindApiPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        var options = new DbContextOptionsBuilder<BrokerOsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BrokerOsDbContext(options, new DesignTimeTenantContext(), new SystemClock());
    }

    private static string FindApiPath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            var apiPath = Path.Combine(current.FullName, "src", "BrokerOS.Api");
            if (File.Exists(Path.Combine(apiPath, "appsettings.json")))
            {
                return apiPath;
            }

            if (File.Exists(Path.Combine(current.FullName, "appsettings.json"))
                && current.Name.Equals("BrokerOS.Api", StringComparison.OrdinalIgnoreCase))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate BrokerOS.Api appsettings.json for design-time EF Core.");
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public long? OrganizationId { get; set; }

        public string? CurrentUserIdentifier { get; set; } = "design-time";
    }
}
