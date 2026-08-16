using BrokerOS.Api.Auth;
using BrokerOS.Api.Filters;
using BrokerOS.Api.Middleware;
using BrokerOS.Application;
using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Security;
using BrokerOS.Infrastructure;
using BrokerOS.Infrastructure.Auth;
using BrokerOS.Infrastructure.Persistence;
using BrokerOS.Infrastructure.Persistence.Seed;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, _, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddValidatorsFromAssembly(typeof(BrokerOS.Application.DependencyInjection).Assembly);
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<FluentValidationActionFilter>();
        options.Filters.Add(new AuthorizeFilter());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    builder.Services.AddScoped<FluentValidationActionFilter>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "BrokerOS API",
            Version = "v1",
            Description = "Insurance Broker Operations & Renewal Automation Platform"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the JWT access token. Swagger adds the Bearer prefix."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    builder.Services.AddHealthChecks();

    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
    {
        throw new InvalidOperationException("Jwt:Key must be configured and at least 32 characters long.");
    }

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType = JwtTokenService.RoleClaim,
                NameClaimType = JwtTokenService.EmailClaim
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var payload = ApiResponse.Fail("Authentication is required.", traceId: context.HttpContext.TraceIdentifier);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var payload = ApiResponse.Fail("You do not have permission to perform this action.", traceId: context.HttpContext.TraceIdentifier);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthPolicies.AdminOnly, policy =>
            policy.RequireRole(AuthPolicies.Roles.BrokerAdmin));
        options.AddPolicy(AuthPolicies.CanManageOrganization, policy =>
            policy.RequireRole(AuthPolicies.Roles.BrokerAdmin));
        options.AddPolicy(AuthPolicies.CanManageOperations, policy =>
            policy.RequireRole(AuthPolicies.Roles.BrokerAdmin, AuthPolicies.Roles.BrokerManager));
        options.AddPolicy(AuthPolicies.CanCreateActivities, policy =>
            policy.RequireRole(
                AuthPolicies.Roles.BrokerAdmin,
                AuthPolicies.Roles.BrokerManager,
                AuthPolicies.Roles.BrokerEmployee));
        options.AddPolicy(AuthPolicies.CanUpdateAssignedWork, policy =>
            policy.RequireRole(
                AuthPolicies.Roles.BrokerAdmin,
                AuthPolicies.Roles.BrokerManager,
                AuthPolicies.Roles.BrokerEmployee));
    });

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        var connection = app.Configuration.GetConnectionString("DefaultConnection") ?? "(none)";
        var server = connection
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(part => part.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
            ?? "Server=(missing)";
        Log.Information("Development SQL {Server}", server);

        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BrokerOsDbContext>();
            await db.Database.MigrateAsync();
            var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
            await seeder.SeedAsync();
            Log.Information("Development database is ready.");
        }
        catch (Exception seedException)
        {
            Log.Warning(seedException, "Development database setup skipped because SQL Server is not available. {Server}", server);
        }
    }

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    var enableSwagger = app.Environment.IsDevelopment()
        || app.Configuration.GetValue("BrokerOS:EnableSwagger", false);

    if (enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "BrokerOS API v1");
            options.DocumentTitle = "BrokerOS API";
        });
    }

    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseMiddleware<TenantResolutionMiddleware>();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Starting BrokerOS API");
    app.Run();
}
catch (HostAbortedException)
{
    throw;
}
catch (Exception exception)
{
    Log.Fatal(exception, "BrokerOS API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
}
