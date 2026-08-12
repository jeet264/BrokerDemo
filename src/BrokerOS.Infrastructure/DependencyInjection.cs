using BrokerOS.Application.Abstractions;
using BrokerOS.Infrastructure.Persistence;
using BrokerOS.Infrastructure.Tenancy;
using BrokerOS.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrokerOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ITenantContext, TenantContext>();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<BrokerOsDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
