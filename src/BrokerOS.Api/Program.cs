using BrokerOS.Api.Filters;
using BrokerOS.Api.Middleware;
using BrokerOS.Application;
using BrokerOS.Infrastructure;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;

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

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<FluentValidationActionFilter>();
    });
    builder.Services.AddScoped<FluentValidationActionFilter>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "BrokerOS API",
            Version = "v1",
            Description = "Insurance Broker Operations & Renewal Automation Platform"
        });
    });
    builder.Services.AddHealthChecks();

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
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Starting BrokerOS API");
    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "BrokerOS API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
