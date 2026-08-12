using BrokerOS.Application.Abstractions;
using BrokerOS.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrokerOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
