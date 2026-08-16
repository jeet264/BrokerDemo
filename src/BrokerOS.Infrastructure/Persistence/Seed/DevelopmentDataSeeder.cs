using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Time;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BrokerOS.Infrastructure.Persistence.Seed;

/// <summary>
/// Development-only demo data (Apex brokers, three roles, global Indian insurers).
/// Runs on API startup when ASPNETCORE_ENVIRONMENT=Development and SQL is reachable. Never used in production.
/// </summary>
public sealed class DevelopmentDataSeeder
{
    public const string DemoOrganizationCode = "APEX";
    public const string DemoPassword = "Demo@12345";

    private readonly BrokerOsDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IClock _clock;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        BrokerOsDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IClock clock,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _clock = clock;
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
        await EnsureMyDayDemoAsync(organization, cancellationToken);

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

    /// <summary>
    /// Sample book so My Day is not empty on a fresh Development database.
    /// Dates are relative to IST today so overdue / due-today / 7-day escalation stay in the right bucket.
    /// </summary>
    private async Task EnsureMyDayDemoAsync(Organization organization, CancellationToken cancellationToken)
    {
        var alreadySeeded = await _dbContext.Clients
            .IgnoreQueryFilters()
            .AnyAsync(
                client => client.OrganizationId == organization.Id && client.ClientCode == "MYDAY-SUNRISE",
                cancellationToken);

        if (alreadySeeded)
        {
            return;
        }

        var employee = await _dbContext.Users
            .IgnoreQueryFilters()
            .SingleAsync(user => user.Email == "employee@apexbrokers.in", cancellationToken);
        var manager = await _dbContext.Users
            .IgnoreQueryFilters()
            .SingleAsync(user => user.Email == "manager@apexbrokers.in", cancellationToken);
        var insurer = await _dbContext.Insurers
            .IgnoreQueryFilters()
            .SingleAsync(item => item.OrganizationId == null && item.Code == "ICICIL", cancellationToken);

        var today = IndiaBusinessCalendar.IstToday(_clock.UtcNow);

        var sunrise = DemoClient(organization.Id, employee.Id, "MYDAY-SUNRISE", "Sunrise Textiles Pvt Ltd", "9876500001", "Mumbai", "Maharashtra");
        var harbor = DemoClient(organization.Id, manager.Id, "MYDAY-HARBOR", "Harbor Logistics", "9876500002", "Chennai", "Tamil Nadu");
        var meadow = DemoClient(organization.Id, employee.Id, "MYDAY-MEADOW", "Meadow Health Clinic", "9876500003", "Pune", "Maharashtra");
        var peak = DemoClient(organization.Id, assignedUserId: null, "MYDAY-PEAK", "Peak Engineering", "9876500004", "Ahmedabad", "Gujarat");

        _dbContext.Clients.AddRange(sunrise, harbor, meadow, peak);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var overduePolicy = DemoPolicy(organization.Id, sunrise.Id, insurer.Id, employee.Id, "POL-MYDAY-001", PolicyType.Motor, today.AddDays(-12), 185000, today);
        var todayPolicy = DemoPolicy(organization.Id, harbor.Id, insurer.Id, manager.Id, "POL-MYDAY-002", PolicyType.Marine, today.AddYears(-1), 420000, today);
        var escalationPolicy = DemoPolicy(organization.Id, meadow.Id, insurer.Id, employee.Id, "POL-MYDAY-003", PolicyType.Health, today.AddDays(9), 96000, today);
        var soonPolicy = DemoPolicy(organization.Id, peak.Id, insurer.Id, assignedUserId: null, "POL-MYDAY-004", PolicyType.Engineering, today.AddDays(2), 1250000, today);

        _dbContext.Policies.AddRange(overduePolicy, todayPolicy, escalationPolicy, soonPolicy);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _dbContext.Renewals.AddRange(
            new Renewal
            {
                OrganizationId = organization.Id,
                PolicyId = overduePolicy.Id,
                AssignedUserId = employee.Id,
                RenewalDate = today.AddDays(-12),
                Status = RenewalStatus.Overdue,
                Priority = RenewalPriority.Critical,
                CurrentStage = RenewalStage.ClientContact,
                NextFollowUpAtUtc = ToUtcNoonIst(today.AddDays(-2))
            },
            new Renewal
            {
                OrganizationId = organization.Id,
                PolicyId = todayPolicy.Id,
                AssignedUserId = manager.Id,
                RenewalDate = today.AddDays(40),
                Status = RenewalStatus.InProgress,
                Priority = RenewalPriority.High,
                CurrentStage = RenewalStage.QuotationReceived,
                NextFollowUpAtUtc = ToUtcNoonIst(today)
            },
            new Renewal
            {
                OrganizationId = organization.Id,
                PolicyId = escalationPolicy.Id,
                AssignedUserId = employee.Id,
                RenewalDate = today.AddDays(9),
                Status = RenewalStatus.Upcoming,
                Priority = RenewalPriority.High,
                CurrentStage = RenewalStage.NotStarted
            },
            new Renewal
            {
                OrganizationId = organization.Id,
                PolicyId = soonPolicy.Id,
                AssignedUserId = null,
                RenewalDate = today.AddDays(2),
                Status = RenewalStatus.Upcoming,
                Priority = RenewalPriority.Medium,
                CurrentStage = RenewalStage.QuotationRequested
            });

        _dbContext.Tasks.AddRange(
            new WorkTask
            {
                OrganizationId = organization.Id,
                ClientId = sunrise.Id,
                PolicyId = overduePolicy.Id,
                AssignedUserId = employee.Id,
                Title = "Collect RC copy",
                Description = "Insurer asked for the vehicle RC before quoting.",
                DueDateUtc = ToUtcNoonIst(today.AddDays(-5)),
                Priority = TaskPriority.High,
                Status = WorkTaskStatus.Overdue
            },
            new WorkTask
            {
                OrganizationId = organization.Id,
                ClientId = harbor.Id,
                PolicyId = todayPolicy.Id,
                AssignedUserId = manager.Id,
                Title = "Send comparative quote",
                DueDateUtc = ToUtcNoonIst(today),
                Priority = TaskPriority.Critical,
                Status = WorkTaskStatus.Pending
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Development My Day sample renewals and tasks are ready.");
    }

    private static Client DemoClient(
        long organizationId,
        long? assignedUserId,
        string code,
        string name,
        string phone,
        string city,
        string state) =>
        new()
        {
            OrganizationId = organizationId,
            AssignedUserId = assignedUserId,
            ClientCode = code,
            CompanyName = name,
            ClientType = ClientType.Corporate,
            Email = $"{code.ToLowerInvariant()}@demo.apexbrokers.in",
            Phone = phone,
            AddressLine1 = "Demo address",
            City = city,
            State = state,
            PostalCode = "400001",
            Country = "India",
            IsActive = true
        };

    private static Policy DemoPolicy(
        long organizationId,
        long clientId,
        long insurerId,
        long? assignedUserId,
        string number,
        PolicyType type,
        DateOnly expiry,
        decimal premium,
        DateOnly today) =>
        new()
        {
            OrganizationId = organizationId,
            ClientId = clientId,
            InsurerId = insurerId,
            AssignedUserId = assignedUserId,
            PolicyNumber = number,
            PolicyType = type,
            StartDate = expiry.AddYears(-1).AddDays(1),
            ExpiryDate = expiry,
            Premium = premium,
            SumInsured = premium * 40,
            CommissionPercentage = 12.5m,
            CommissionAmount = decimal.Round(premium * 0.125m, 2),
            Status = expiry < today ? PolicyStatus.Expired : PolicyStatus.PendingRenewal
        };

    private static DateTime ToUtcNoonIst(DateOnly istDate)
    {
        var unspecifiedNoon = DateTime.SpecifyKind(istDate.ToDateTime(new TimeOnly(12, 0)), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecifiedNoon, IndiaBusinessCalendar.TimeZone);
    }
}
