using BrokerOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrokerOS.Api.Tests;

public sealed class BrokerOsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string JwtKey = "TEST-ONLY-JWT-KEY-BROKEROS-32CHARS!";

    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("BROKEROS_TEST_CONNECTION")
        ?? "Server=localhost,1433;Database=BrokerOS_Tests;User Id=sa;Password=BrokerOS_Demo_123;TrustServerCertificate=True;Encrypt=True";

    public TestCatalog Catalog { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        builder.UseSetting("Jwt:Issuer", "BrokerOS");
        builder.UseSetting("Jwt:Audience", "BrokerOS.Web");
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:ExpiryHours", "8");
        builder.UseSetting("RenewalWorker:Enabled", "false");
        builder.UseSetting("BrokerOS:EnableDemoReset", "false");
        builder.UseSetting("BrokerOS:EnableSwagger", "false");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:Issuer"] = "BrokerOS",
                ["Jwt:Audience"] = "BrokerOS.Web",
                ["Jwt:Key"] = JwtKey,
                ["Jwt:ExpiryHours"] = "8",
                ["RenewalWorker:Enabled"] = "false",
                ["BrokerOS:EnableDemoReset"] = "false",
                ["BrokerOS:EnableSwagger"] = "false",
                ["Serilog:MinimumLevel:Default"] = "Warning",
                ["Serilog:MinimumLevel:Override:Microsoft"] = "Warning",
                ["Serilog:MinimumLevel:Override:Microsoft.AspNetCore"] = "Warning"
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BrokerOsDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        Catalog = await TestCatalogSeeder.SeedAsync(Services);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
