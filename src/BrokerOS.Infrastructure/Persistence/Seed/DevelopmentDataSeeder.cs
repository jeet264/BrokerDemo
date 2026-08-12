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

        _logger.LogInformation("Development demo users are ready for organization {Code}", DemoOrganizationCode);
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
