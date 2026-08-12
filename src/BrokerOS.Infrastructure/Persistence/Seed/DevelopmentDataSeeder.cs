using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BrokerOS.Infrastructure.Persistence.Seed;

public sealed class DevelopmentDataSeeder
{
    public const string DemoOrganizationCode = "APEX";
    public const string DemoPassword = "Demo@12345";

    private readonly BrokerOsDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        BrokerOsDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var organization = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(entity => entity.Code == DemoOrganizationCode, cancellationToken);

        if (organization is null)
        {
            organization = new Organization
            {
                Name = "Apex Insurance Brokers",
                Code = DemoOrganizationCode,
                IsActive = true
            };
            _dbContext.Organizations.Add(organization);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureUserAsync(organization, "admin@apexbrokers.in", "Apex Admin", UserRole.BrokerAdmin, cancellationToken);
        await EnsureUserAsync(organization, "manager@apexbrokers.in", "Apex Manager", UserRole.BrokerManager, cancellationToken);
        await EnsureUserAsync(organization, "employee@apexbrokers.in", "Apex Employee", UserRole.BrokerEmployee, cancellationToken);
        await EnsureGlobalInsurersAsync(cancellationToken);

        _logger.LogInformation("Development demo users are ready for organization {Code}", DemoOrganizationCode);
    }

    private async Task EnsureGlobalInsurersAsync(CancellationToken cancellationToken)
    {
        var globals = new (string Name, string Code, string Email, string Website)[]
        {
            ("ICICI Lombard", "ICICIL", "support@icicilombard.com", "https://www.icicilombard.com"),
            ("HDFC ERGO", "HDFCERGO", "support@hdfcergo.com", "https://www.hdfcergo.com"),
            ("New India Assurance", "NEWINDIA", "support@newindia.co.in", "https://www.newindia.co.in"),
            ("Bajaj Allianz", "BAJAJAZ", "support@bajajallianz.com", "https://www.bajajallianz.com"),
            ("Star Health", "STARHEALTH", "support@starhealth.in", "https://www.starhealth.in")
        };

        foreach (var (name, code, email, website) in globals)
        {
            var exists = await _dbContext.Insurers
                .IgnoreQueryFilters()
                .AnyAsync(insurer => insurer.OrganizationId == null && insurer.Code == code, cancellationToken);

            if (exists)
            {
                continue;
            }

            _dbContext.Insurers.Add(new Insurer
            {
                OrganizationId = null,
                Name = name,
                Code = code,
                Email = email,
                Website = website,
                IsActive = true
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUserAsync(
        Organization organization,
        string email,
        string fullName,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(entity => entity.Email == email && !entity.IsDeleted, cancellationToken);

        if (exists)
        {
            return;
        }

        var user = new User
        {
            OrganizationId = organization.Id,
            Email = email,
            FullName = fullName,
            Role = role,
            IsActive = true,
            CreatedBy = "seed"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, DemoPassword);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
