using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Import;
using BrokerOS.Application.Security;
using BrokerOS.Infrastructure.Auth;
using BrokerOS.Infrastructure.Clients;
using BrokerOS.Infrastructure.Import;
using BrokerOS.Infrastructure.Insurers;
using BrokerOS.Infrastructure.Organizations;
using BrokerOS.Infrastructure.Persistence;
using BrokerOS.Infrastructure.Persistence.Seed;
using BrokerOS.Infrastructure.Tenancy;
using BrokerOS.Infrastructure.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BrokerOS.Domain.Entities;

namespace BrokerOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IInsurerService, InsurerService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddSingleton<IImportPreviewCache, MemoryImportPreviewCache>();
        services.AddMemoryCache();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<DevelopmentDataSeeder>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<BrokerOsDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
