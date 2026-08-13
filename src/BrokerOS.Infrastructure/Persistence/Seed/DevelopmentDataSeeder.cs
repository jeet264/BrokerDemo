using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Policies;
using BrokerOS.Domain.Renewals;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrokerOS.Infrastructure.Persistence.Seed;

public sealed class DevelopmentDataSeeder
{
    public const string DemoOrganizationCode = "APEX";
    public const string DemoPassword = "Demo@12345";

    private readonly BrokerOsDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IClock _clock;
    private readonly ITenantContext _tenantContext;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        BrokerOsDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IClock clock,
        ITenantContext tenantContext,
        IHostEnvironment environment,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _clock = clock;
        _tenantContext = tenantContext;
        _environment = environment;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogInformation(
                "Development demo seeder skipped because the environment is {Environment}.",
                _environment.EnvironmentName);
            return;
        }

        var organization = await EnsureOrganizationAsync(cancellationToken);
        _tenantContext.OrganizationId = organization.Id;
        _tenantContext.CurrentUserIdentifier = "seed";

        foreach (var user in DevelopmentDemoCatalog.Users)
        {
            await EnsureUserAsync(organization, user.Email, user.FullName, user.Role, cancellationToken);
        }

        var users = await _dbContext.Users
            .IgnoreQueryFilters()
            .Where(user => user.OrganizationId == organization.Id && !user.IsDeleted)
            .ToListAsync(cancellationToken);
        var admin = users.Single(user => user.Email == "admin@apexbrokers.in");
        var assignees = users
            .Where(user => user.Role is UserRole.BrokerManager or UserRole.BrokerEmployee)
            .OrderBy(user => user.Role)
            .ThenBy(user => user.Email)
            .ToList();

        var insurers = await EnsureInsurersAsync(organization, cancellationToken);
        var clients = await EnsureClientsAsync(organization, assignees, cancellationToken);
        await EnsurePoliciesAsync(organization, clients, insurers, assignees, admin, cancellationToken);

        _logger.LogInformation(
            "Development demo data is ready for {Code}: {Users} users, {Insurers} insurers, {Clients} clients, {Policies} policies.",
            DemoOrganizationCode,
            DevelopmentDemoCatalog.Users.Length,
            DevelopmentDemoCatalog.Insurers.Length,
            DevelopmentDemoCatalog.ClientCount,
            DevelopmentDemoCatalog.PolicyCount);
    }

    private async Task<Organization> EnsureOrganizationAsync(CancellationToken cancellationToken)
    {
        var organization = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(entity => entity.Code == DemoOrganizationCode, cancellationToken);

        if (organization is not null)
        {
            return organization;
        }

        organization = new Organization
        {
            Name = "Apex Insurance Brokers",
            Code = DemoOrganizationCode,
            IsActive = true
        };
        _dbContext.Organizations.Add(organization);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return organization;
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

    private async Task<IReadOnlyList<Insurer>> EnsureInsurersAsync(
        Organization organization,
        CancellationToken cancellationToken)
    {
        foreach (var spec in DevelopmentDemoCatalog.Insurers)
        {
            var existing = await _dbContext.Insurers
                .IgnoreQueryFilters()
                .Where(entity => entity.Code == spec.Code)
                .OrderByDescending(entity => entity.OrganizationId == organization.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing is not null)
            {
                continue;
            }

            _dbContext.Insurers.Add(new Insurer
            {
                OrganizationId = organization.Id,
                Name = spec.Name,
                Code = spec.Code,
                Email = spec.Email,
                Phone = spec.Phone,
                Website = spec.Website,
                IsActive = true
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var insurers = new List<Insurer>();
        foreach (var spec in DevelopmentDemoCatalog.Insurers)
        {
            var insurer = await _dbContext.Insurers
                .IgnoreQueryFilters()
                .Where(entity => entity.Code == spec.Code)
                .OrderByDescending(entity => entity.OrganizationId == organization.Id)
                .FirstAsync(cancellationToken);
            insurers.Add(insurer);
        }

        return insurers;
    }

    private async Task<IReadOnlyList<Client>> EnsureClientsAsync(
        Organization organization,
        IReadOnlyList<User> assignees,
        CancellationToken cancellationToken)
    {
        if (DevelopmentDemoCatalog.Clients.Length != DevelopmentDemoCatalog.ClientCount)
        {
            throw new InvalidOperationException("Development demo catalog must define exactly 50 clients.");
        }

        var existing = await _dbContext.Clients
            .IgnoreQueryFilters()
            .Include(client => client.Contacts)
            .Where(client => client.OrganizationId == organization.Id && !client.IsDeleted)
            .ToListAsync(cancellationToken);
        var byCode = existing.ToDictionary(client => client.ClientCode, StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < DevelopmentDemoCatalog.Clients.Length; index++)
        {
            var spec = DevelopmentDemoCatalog.Clients[index];
            var code = DevelopmentDemoCatalog.ClientCode(index);
            if (byCode.TryGetValue(code, out var existing))
            {
                existing.CompanyName = spec.CompanyName;
                existing.ClientType = spec.ClientType;
                existing.Industry = spec.Industry;
                existing.AddressLine1 = spec.AddressLine1;
                existing.City = spec.City;
                existing.State = spec.State;
                existing.PostalCode = spec.PostalCode;
                existing.Country = "India";
                if (string.IsNullOrWhiteSpace(existing.Notes))
                {
                    existing.Notes = "Development demo client. Contact details are fictional.";
                }

                continue;
            }

            var assignee = assignees[index % assignees.Count];
            var client = new Client
            {
                OrganizationId = organization.Id,
                ClientCode = code,
                CompanyName = spec.CompanyName,
                ClientType = spec.ClientType,
                Industry = spec.Industry,
                Email = DevelopmentDemoCatalog.CompanyEmail(spec.CompanyName, index),
                Phone = DevelopmentDemoCatalog.DemoPhone(index),
                AddressLine1 = spec.AddressLine1,
                City = spec.City,
                State = spec.State,
                PostalCode = spec.PostalCode,
                Country = "India",
                AssignedUserId = assignee.Id,
                IsActive = true,
                CreatedBy = "seed",
                Notes = "Development demo client. Contact details are fictional."
            };
            _dbContext.Clients.Add(client);
            byCode[code] = client;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < DevelopmentDemoCatalog.Clients.Length; index++)
        {
            var client = byCode[DevelopmentDemoCatalog.ClientCode(index)];
            if (client.Contacts.Any(contact => !contact.IsDeleted))
            {
                continue;
            }

            _dbContext.Contacts.Add(new Contact
            {
                OrganizationId = organization.Id,
                ClientId = client.Id,
                FirstName = DevelopmentDemoCatalog.ContactFirstNames[index % DevelopmentDemoCatalog.ContactFirstNames.Length],
                LastName = DevelopmentDemoCatalog.ContactLastNames[index % DevelopmentDemoCatalog.ContactLastNames.Length],
                Designation = DevelopmentDemoCatalog.ContactDesignations[index % DevelopmentDemoCatalog.ContactDesignations.Length],
                Email = client.Email,
                Phone = client.Phone,
                IsPrimary = true
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return byCode.Values.ToList();
    }

    private async Task EnsurePoliciesAsync(
        Organization organization,
        IReadOnlyList<Client> clients,
        IReadOnlyList<Insurer> insurers,
        IReadOnlyList<User> assignees,
        User admin,
        CancellationToken cancellationToken)
    {
        var today = _clock.Today;
        var created = new List<Policy>();

        for (var index = 0; index < DevelopmentDemoCatalog.PolicyCount; index++)
        {
            var policyNumber = DevelopmentDemoCatalog.PolicyNumber(index);
            var exists = await _dbContext.Policies
                .IgnoreQueryFilters()
                .AnyAsync(
                    policy => policy.OrganizationId == organization.Id && policy.PolicyNumber == policyNumber && !policy.IsDeleted,
                    cancellationToken);

            if (exists)
            {
                continue;
            }

            created.Add(CreateCatalogPolicy(organization, clients, insurers, assignees, index, today));
        }

        if (created.Count > 0)
        {
            _dbContext.Policies.AddRange(created);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureCompletedNextTermsAsync(organization, cancellationToken);
        await EnsureWorkAsync(organization, admin, cancellationToken);
    }

    private Policy CreateCatalogPolicy(
        Organization organization,
        IReadOnlyList<Client> clients,
        IReadOnlyList<Insurer> insurers,
        IReadOnlyList<User> assignees,
        int index,
        DateOnly today)
    {
        var clientCode = DevelopmentDemoCatalog.ClientCode(DevelopmentDemoCatalog.ClientIndexForPolicy(index));
        var client = clients.Single(item => item.ClientCode == clientCode);
        var insurer = insurers[index % insurers.Count];
        var policyType = DevelopmentDemoCatalog.PolicyTypes[index % DevelopmentDemoCatalog.PolicyTypes.Length];
        var bucket = DevelopmentDemoCatalog.BucketFor(index);
        var expiry = DevelopmentDemoCatalog.ExpiryFor(index, today);
        var (premium, sumInsured, commissionPercentage) = DevelopmentDemoCatalog.MoneyFor(policyType, index);
        var assignedUserId = index % 11 == 0
            ? assignees[(index / 11) % assignees.Count].Id
            : client.AssignedUserId ?? assignees[0].Id;
        var daysRemaining = expiry.DayNumber - today.DayNumber;

        var policy = new Policy
        {
            OrganizationId = organization.Id,
            ClientId = client.Id,
            InsurerId = insurer.Id,
            PolicyNumber = DevelopmentDemoCatalog.PolicyNumber(index),
            PolicyType = policyType,
            StartDate = expiry.AddYears(-1),
            ExpiryDate = expiry,
            Premium = premium,
            SumInsured = sumInsured,
            CommissionPercentage = commissionPercentage,
            CommissionAmount = CommissionCalculator.Amount(premium, commissionPercentage),
            AssignedUserId = assignedUserId,
            Status = bucket switch
            {
                DemoRenewalBucket.Completed => PolicyStatus.Expired,
                DemoRenewalBucket.Lost => PolicyStatus.Cancelled,
                _ => PolicyStatus.Active
            },
            CreatedBy = "seed",
            Notes = $"Development demo {bucket} policy."
        };

        var renewal = new Renewal
        {
            OrganizationId = organization.Id,
            AssignedUserId = assignedUserId,
            RenewalDate = expiry,
            Status = StatusFor(bucket, daysRemaining),
            CurrentStage = StageFor(bucket, index),
            Priority = RenewalMilestones.RenewalPriorityFor(daysRemaining),
            CreatedBy = "seed",
            LastFollowUpAtUtc = bucket is DemoRenewalBucket.Later ? null : _clock.UtcNow.AddDays(-Math.Max(1, 12 - index % 10)),
            NextFollowUpAtUtc = bucket is DemoRenewalBucket.Completed or DemoRenewalBucket.Lost
                ? null
                : _clock.UtcNow.AddDays(2 + index % 5)
        };
        policy.Renewals.Add(renewal);
        return policy;
    }

    private async Task EnsureCompletedNextTermsAsync(Organization organization, CancellationToken cancellationToken)
    {
        var today = _clock.Today;
        var completed = await _dbContext.Policies
            .IgnoreQueryFilters()
            .Include(policy => policy.Renewals)
            .Where(policy =>
                policy.OrganizationId == organization.Id
                && !policy.IsDeleted
                && policy.CreatedBy == "seed"
                && policy.Status == PolicyStatus.Expired
                && policy.NextPolicyId == null
                && policy.PolicyNumber.StartsWith("POL-D")
                && !policy.PolicyNumber.Contains("-R"))
            .ToListAsync(cancellationToken);

        var nextPolicies = new List<(Policy Expired, Policy Next)>();
        foreach (var expired in completed)
        {
            var nextNumber = $"{expired.PolicyNumber}-R2";
            var alreadyRolled = await _dbContext.Policies
                .IgnoreQueryFilters()
                .AnyAsync(
                    policy => policy.OrganizationId == organization.Id && policy.PolicyNumber == nextNumber && !policy.IsDeleted,
                    cancellationToken);
            if (alreadyRolled)
            {
                continue;
            }

            var nextStart = expired.ExpiryDate.AddDays(1);
            var nextPremium = RoundPremium(expired.Premium * 1.04m);
            var nextPolicy = new Policy
            {
                OrganizationId = organization.Id,
                ClientId = expired.ClientId,
                InsurerId = expired.InsurerId,
                PolicyNumber = nextNumber,
                PolicyType = expired.PolicyType,
                StartDate = nextStart,
                ExpiryDate = nextStart.AddYears(1),
                Premium = nextPremium,
                SumInsured = expired.SumInsured,
                CommissionPercentage = expired.CommissionPercentage,
                CommissionAmount = CommissionCalculator.Amount(nextPremium, expired.CommissionPercentage),
                AssignedUserId = expired.AssignedUserId,
                Status = PolicyStatus.Active,
                PreviousPolicyId = expired.Id,
                CreatedBy = "seed",
                Notes = $"Next term after {expired.PolicyNumber}."
            };
            nextPolicy.Renewals.Add(RenewalFactory.CreateForPolicy(nextPolicy, today));
            _dbContext.Policies.Add(nextPolicy);
            nextPolicies.Add((expired, nextPolicy));
        }

        if (nextPolicies.Count == 0)
        {
            return;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (expired, nextPolicy) in nextPolicies)
        {
            expired.NextPolicyId = nextPolicy.Id;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureWorkAsync(Organization organization, User fallbackUser, CancellationToken cancellationToken)
    {
        var policies = await _dbContext.Policies
            .IgnoreQueryFilters()
            .Include(policy => policy.Renewals)
            .Include(policy => policy.Client)
            .Where(policy =>
                policy.OrganizationId == organization.Id
                && !policy.IsDeleted
                && policy.CreatedBy == "seed"
                && policy.PolicyNumber.StartsWith("POL-D"))
            .ToListAsync(cancellationToken);

        var seededPolicyIds = policies.Select(policy => policy.Id).ToList();
        var existingActivityPolicyIds = await _dbContext.Activities
            .IgnoreQueryFilters()
            .Where(activity =>
                activity.PolicyId != null
                && seededPolicyIds.Contains(activity.PolicyId.Value)
                && activity.ActivityType == ActivityType.PolicyCreated)
            .Select(activity => activity.PolicyId!.Value)
            .ToListAsync(cancellationToken);
        var alreadySeeded = existingActivityPolicyIds.ToHashSet();

        foreach (var policy in policies)
        {
            if (alreadySeeded.Contains(policy.Id) || policy.PolicyNumber.EndsWith("-R2", StringComparison.Ordinal))
            {
                continue;
            }

            var renewal = policy.Renewals.OrderBy(item => item.Id).FirstOrDefault();
            if (renewal is null)
            {
                continue;
            }

            var actorId = policy.AssignedUserId ?? fallbackUser.Id;
            AddActivity(policy, renewal, actorId, ActivityType.PolicyCreated, $"Policy {policy.PolicyNumber} created.");
            AddActivity(
                policy,
                renewal,
                actorId,
                ActivityType.RenewalCreated,
                $"Renewal opened for {policy.PolicyNumber} expiring {policy.ExpiryDate:yyyy-MM-dd}.");

            if (renewal.Status is RenewalStatus.Renewed)
            {
                AddActivity(
                    policy,
                    renewal,
                    actorId,
                    ActivityType.PolicyRenewed,
                    $"Policy {policy.PolicyNumber} renewed. Next term {policy.PolicyNumber}-R2 is on cover.");
                AddCompletedTask(policy, renewal, actorId, "Close renewal file and issue policy copy");
                continue;
            }

            if (renewal.Status is RenewalStatus.Lost)
            {
                AddActivity(
                    policy,
                    renewal,
                    actorId,
                    ActivityType.RenewalLost,
                    "Client placed cover directly with the insurer. File marked lost.");
                AddCompletedTask(policy, renewal, actorId, "Record lost reason and close file");
                continue;
            }

            AddActivity(
                policy,
                renewal,
                actorId,
                ActivityType.Call,
                $"Called {policy.Client.CompanyName} operations desk to confirm the renewal diary.");
            AddActivity(
                policy,
                renewal,
                actorId,
                ActivityType.Email,
                $"Emailed a renewal reminder for {policy.PolicyNumber} ({policy.PolicyType}).");

            if (renewal.Status is RenewalStatus.Overdue or RenewalStatus.InProgress or RenewalStatus.QuotationPending)
            {
                AddActivity(
                    policy,
                    renewal,
                    actorId,
                    ActivityType.ClientContact,
                    "Discussed cover changes, claims in the term, and target premium.");
                AddOpenTask(
                    policy,
                    renewal,
                    actorId,
                    renewal.Status == RenewalStatus.Overdue
                        ? "Urgent: recover overdue renewal"
                        : "Follow up with client on renewal decision",
                    renewal.Status == RenewalStatus.Overdue ? TaskPriority.Critical : TaskPriority.High,
                    dueDays: renewal.Status == RenewalStatus.Overdue ? -1 : 2);
            }

            if (renewal.CurrentStage is RenewalStage.QuotationRequested or RenewalStage.QuotationReceived)
            {
                AddActivity(
                    policy,
                    renewal,
                    actorId,
                    ActivityType.InsurerContact,
                    "Requested renewal terms and comparable quotations.");
                AddOpenTask(
                    policy,
                    renewal,
                    actorId,
                    "Chase insurer quotation",
                    TaskPriority.High,
                    dueDays: 1);
            }

            if (policy.ExpiryDate.DayNumber - _clock.Today.DayNumber is >= 1 and <= 7)
            {
                AddActivity(
                    policy,
                    renewal,
                    actorId,
                    ActivityType.WhatsApp,
                    "Sent a WhatsApp reminder that cover expires this week.");
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AddActivity(Policy policy, Renewal renewal, long userId, ActivityType type, string description)
    {
        _dbContext.Activities.Add(new Activity
        {
            OrganizationId = policy.OrganizationId,
            ClientId = policy.ClientId,
            PolicyId = policy.Id,
            RenewalId = renewal.Id,
            UserId = userId,
            ActivityType = type,
            Description = description
        });
    }

    private void AddOpenTask(
        Policy policy,
        Renewal renewal,
        long assignedUserId,
        string title,
        TaskPriority priority,
        int dueDays)
    {
        _dbContext.Tasks.Add(new WorkTask
        {
            OrganizationId = policy.OrganizationId,
            RenewalId = renewal.Id,
            ClientId = policy.ClientId,
            PolicyId = policy.Id,
            AssignedUserId = assignedUserId,
            Title = title,
            Description = $"{title} for {policy.PolicyNumber} ({policy.Client.CompanyName}).",
            DueDateUtc = _clock.UtcNow.Date.AddDays(dueDays).AddHours(11),
            Priority = priority,
            Status = WorkTaskStatus.Pending,
            CreatedBy = "seed"
        });
    }

    private void AddCompletedTask(Policy policy, Renewal renewal, long assignedUserId, string title)
    {
        _dbContext.Tasks.Add(new WorkTask
        {
            OrganizationId = policy.OrganizationId,
            RenewalId = renewal.Id,
            ClientId = policy.ClientId,
            PolicyId = policy.Id,
            AssignedUserId = assignedUserId,
            Title = title,
            Description = $"{title} for {policy.PolicyNumber}.",
            DueDateUtc = _clock.UtcNow.AddDays(-3),
            CompletedAtUtc = _clock.UtcNow.AddDays(-2),
            Priority = TaskPriority.Medium,
            Status = WorkTaskStatus.Completed,
            CreatedBy = "seed"
        });
    }

    private static RenewalStatus StatusFor(DemoRenewalBucket bucket, int daysRemaining) => bucket switch
    {
        DemoRenewalBucket.Overdue => RenewalStatus.Overdue,
        DemoRenewalBucket.DueToday => RenewalStatus.InProgress,
        DemoRenewalBucket.DueWithin7Days => daysRemaining <= 3 ? RenewalStatus.ClientDecisionPending : RenewalStatus.QuotationPending,
        DemoRenewalBucket.DueWithin30Days => RenewalStatus.InProgress,
        DemoRenewalBucket.Completed => RenewalStatus.Renewed,
        DemoRenewalBucket.Lost => RenewalStatus.Lost,
        _ => RenewalStatus.Upcoming
    };

    private static RenewalStage StageFor(DemoRenewalBucket bucket, int index) => bucket switch
    {
        DemoRenewalBucket.Overdue => index % 2 == 0 ? RenewalStage.ClientContact : RenewalStage.QuotationRequested,
        DemoRenewalBucket.DueToday => RenewalStage.ClientContact,
        DemoRenewalBucket.DueWithin7Days => index % 2 == 0 ? RenewalStage.QuotationReceived : RenewalStage.ClientDecision,
        DemoRenewalBucket.DueWithin30Days => RenewalStage.QuotationRequested,
        DemoRenewalBucket.DueWithin60Days => RenewalStage.ClientContact,
        DemoRenewalBucket.Completed => RenewalStage.Completed,
        DemoRenewalBucket.Lost => RenewalStage.ClientDecision,
        _ => RenewalStage.NotStarted
    };

    private static decimal RoundPremium(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
