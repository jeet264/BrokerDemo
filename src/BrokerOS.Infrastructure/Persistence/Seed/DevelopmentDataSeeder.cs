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
        await EnsureDemoRenewalDataAsync(organization, cancellationToken);

        _logger.LogInformation("Development demo users are ready for organization {Code}", DemoOrganizationCode);
    }

    private async Task EnsureDemoRenewalDataAsync(Organization organization, CancellationToken cancellationToken)
    {
        var manager = await _dbContext.Users
            .IgnoreQueryFilters()
            .SingleAsync(user => user.Email == "manager@apexbrokers.in" && !user.IsDeleted, cancellationToken);
        var employee = await _dbContext.Users
            .IgnoreQueryFilters()
            .SingleAsync(user => user.Email == "employee@apexbrokers.in" && !user.IsDeleted, cancellationToken);

        var insurer = await _dbContext.Insurers
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                entity => entity.OrganizationId == organization.Id && entity.Code == "ICICIL",
                cancellationToken);

        if (insurer is null)
        {
            insurer = await _dbContext.Insurers
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    entity => entity.OrganizationId == null && entity.Code == "ICICIL",
                    cancellationToken);
        }

        if (insurer is null)
        {
            insurer = new Insurer
            {
                OrganizationId = organization.Id,
                Name = "ICICI Lombard",
                Code = "ICICIL",
                Email = "support@icicilombard.com",
                Website = "https://www.icicilombard.com",
                IsActive = true
            };
            _dbContext.Insurers.Add(insurer);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var client = await _dbContext.Clients
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                entity => entity.OrganizationId == organization.Id && entity.ClientCode == "CLI-001" && !entity.IsDeleted,
                cancellationToken);

        if (client is null)
        {
            client = new Client
            {
                OrganizationId = organization.Id,
                ClientCode = "CLI-001",
                CompanyName = "Sharma Logistics Pvt Ltd",
                ClientType = ClientType.Corporate,
                Industry = "Logistics",
                Email = "ops@sharmalogistics.in",
                Phone = "+91 98765 43210",
                AddressLine1 = "12 Andheri East",
                City = "Mumbai",
                State = "Maharashtra",
                PostalCode = "400069",
                Country = "India",
                AssignedUserId = manager.Id,
                IsActive = true,
                CreatedBy = "seed"
            };
            _dbContext.Clients.Add(client);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var policies = new (string Number, PolicyType Type, DateOnly Expiry, decimal Premium, decimal SumInsured, long? AssignedUserId)[]
        {
            ("POL-OVERDUE", PolicyType.Motor, today.AddDays(-3), 250000m, 2500000m, manager.Id),
            ("POL-TODAY", PolicyType.Property, today, 180000m, 5000000m, manager.Id),
            ("POL-7D", PolicyType.Health, today.AddDays(7), 120000m, 1000000m, employee.Id),
            ("POL-15D", PolicyType.Liability, today.AddDays(15), 90000m, 2000000m, manager.Id),
            ("POL-30D", PolicyType.Marine, today.AddDays(30), 310000m, 8000000m, manager.Id),
            ("POL-60D", PolicyType.Engineering, today.AddDays(60), 75000m, 1500000m, manager.Id)
        };

        foreach (var (number, type, expiry, premium, sumInsured, assignedUserId) in policies)
        {
            var exists = await _dbContext.Policies
                .IgnoreQueryFilters()
                .AnyAsync(
                    policy => policy.OrganizationId == organization.Id && policy.PolicyNumber == number && !policy.IsDeleted,
                    cancellationToken);

            if (exists)
            {
                continue;
            }

            _dbContext.Policies.Add(new Policy
            {
                OrganizationId = organization.Id,
                ClientId = client.Id,
                InsurerId = insurer.Id,
                PolicyNumber = number,
                PolicyType = type,
                StartDate = expiry.AddYears(-1),
                ExpiryDate = expiry,
                Premium = premium,
                SumInsured = sumInsured,
                CommissionPercentage = 10m,
                CommissionAmount = Math.Round(premium * 0.10m, 2),
                AssignedUserId = assignedUserId,
                Status = PolicyStatus.Active,
                CreatedBy = "seed"
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
