using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Security;
using BrokerOS.Infrastructure.Auth;
using BrokerOS.Infrastructure.Clients;
using BrokerOS.Infrastructure.Dashboard;
using BrokerOS.Infrastructure.Organizations;
using BrokerOS.Infrastructure.Policies;
using BrokerOS.Infrastructure.Renewals;
using BrokerOS.Infrastructure.Persistence;
using BrokerOS.Infrastructure.Persistence.Seed;
using BrokerOS.Infrastructure.Tenancy;
using BrokerOS.Infrastructure.Time;
using BrokerOS.Infrastructure.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRenewalService, RenewalService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<DevelopmentDataSeeder>();
        services.Configure<RenewalWorkerOptions>(configuration.GetSection(RenewalWorkerOptions.SectionName));
        services.AddHostedService<RenewalReminderWorker>();

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
