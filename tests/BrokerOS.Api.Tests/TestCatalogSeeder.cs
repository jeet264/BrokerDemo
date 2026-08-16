using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Policies;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BrokerOS.Api.Tests;

internal static class TestCatalogSeeder
{
    public static async Task<TestCatalog> SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BrokerOsDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var tenant = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenant.CurrentUserIdentifier = "tests";

        var today = clock.Today;
        var orgA = new Organization { Name = "Test Apex Brokers", Code = "TSTA", IsActive = true };
        var orgB = new Organization { Name = "Test Beta Brokers", Code = "TSTB", IsActive = true };
        db.Organizations.AddRange(orgA, orgB);
        await db.SaveChangesAsync();

        var adminA = CreateUser(orgA.Id, TestUsers.AdminA, "Admin A", UserRole.BrokerAdmin, hasher);
        var managerA = CreateUser(orgA.Id, TestUsers.ManagerA, "Manager A", UserRole.BrokerManager, hasher);
        var employeeA = CreateUser(orgA.Id, TestUsers.EmployeeA, "Employee A", UserRole.BrokerEmployee, hasher);
        var employee2A = CreateUser(orgA.Id, TestUsers.Employee2A, "Employee Two A", UserRole.BrokerEmployee, hasher);
        var adminB = CreateUser(orgB.Id, TestUsers.AdminB, "Admin B", UserRole.BrokerAdmin, hasher);
        db.Users.AddRange(adminA, managerA, employeeA, employee2A, adminB);
        await db.SaveChangesAsync();

        var insurerA = new Insurer
        {
            OrganizationId = orgA.Id,
            Name = "Test New India",
            Code = "TNIA",
            Email = "desk@tnia.test",
            IsActive = true
        };
        var insurerB = new Insurer
        {
            OrganizationId = orgB.Id,
            Name = "Test Oriental",
            Code = "TORB",
            Email = "desk@torb.test",
            IsActive = true
        };
        db.Insurers.AddRange(insurerA, insurerB);

        var clientA1 = CreateClient(orgA.Id, "CLIA1", "Alpha Logistics", employeeA.Id);
        var clientA2 = CreateClient(orgA.Id, "CLIA2", "Beta Traders", employee2A.Id);
        var clientB = CreateClient(orgB.Id, "CLIB1", "Gamma Shipping", adminB.Id);
        db.Clients.AddRange(clientA1, clientA2, clientB);
        await db.SaveChangesAsync();

        const decimal nearPremium = 100_000m;
        const decimal farPremium = 250_000m;
        var policyNear = CreatePolicy(orgA.Id, clientA1.Id, insurerA.Id, employeeA.Id, "POL-A-NEAR", today.AddDays(-335), today.AddDays(30), nearPremium);
        var policyFar = CreatePolicy(orgA.Id, clientA2.Id, insurerA.Id, employee2A.Id, "POL-A-FAR", today.AddDays(-245), today.AddDays(120), farPremium);
        var policyB = CreatePolicy(orgB.Id, clientB.Id, insurerB.Id, adminB.Id, "POL-B-NEAR", today.AddDays(-350), today.AddDays(15), 50_000m);
        db.Policies.AddRange(policyNear, policyFar, policyB);
        await db.SaveChangesAsync();

        var renewalNear = await db.Renewals.IgnoreQueryFilters()
            .SingleAsync(renewal => renewal.PolicyId == policyNear.Id);
        var renewalFar = await db.Renewals.IgnoreQueryFilters()
            .SingleAsync(renewal => renewal.PolicyId == policyFar.Id);
        var renewalB = await db.Renewals.IgnoreQueryFilters()
            .SingleAsync(renewal => renewal.PolicyId == policyB.Id);

        var task = new WorkTask
        {
            OrganizationId = orgA.Id,
            RenewalId = renewalNear.Id,
            ClientId = clientA1.Id,
            PolicyId = policyNear.Id,
            AssignedUserId = employeeA.Id,
            Title = "Call Alpha Logistics",
            Description = "Confirm renewal intent.",
            DueDateUtc = clock.UtcNow.AddDays(1),
            Priority = TaskPriority.High,
            Status = WorkTaskStatus.Pending
        };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        return new TestCatalog
        {
            OrgAClientAssignedPublicId = clientA1.PublicId,
            OrgAClientOtherPublicId = clientA2.PublicId,
            OrgAPolicyNearPublicId = policyNear.PublicId,
            OrgAPolicyFarPublicId = policyFar.PublicId,
            OrgARenewalNearPublicId = renewalNear.PublicId,
            OrgARenewalNearId = renewalNear.Id,
            OrgATaskPublicId = task.PublicId,
            OrgAInsurerPublicId = insurerA.PublicId,
            OrgAEmployeePublicId = employeeA.PublicId,
            OrgBClientPublicId = clientB.PublicId,
            OrgBPolicyPublicId = policyB.PublicId,
            OrgBRenewalPublicId = renewalB.PublicId,
            NearPremium = nearPremium,
            FarPremium = farPremium
        };
    }

    private static User CreateUser(
        long organizationId,
        string email,
        string fullName,
        UserRole role,
        IPasswordHasher<User> hasher)
    {
        var user = new User
        {
            OrganizationId = organizationId,
            Email = email,
            FullName = fullName,
            Role = role,
            IsActive = true
        };
        user.PasswordHash = hasher.HashPassword(user, TestUsers.Password);
        return user;
    }

    private static Client CreateClient(long organizationId, string code, string name, long assignedUserId) =>
        new()
        {
            OrganizationId = organizationId,
            ClientCode = code,
            CompanyName = name,
            ClientType = ClientType.Corporate,
            Industry = "Logistics",
            Email = $"{code.ToLowerInvariant()}@brokeros.test",
            Phone = "+91 90000 00001",
            AddressLine1 = "1 Test Street",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            Country = "India",
            AssignedUserId = assignedUserId,
            IsActive = true
        };

    private static Policy CreatePolicy(
        long organizationId,
        long clientId,
        long insurerId,
        long assignedUserId,
        string number,
        DateOnly start,
        DateOnly expiry,
        decimal premium)
    {
        var policy = new Policy
        {
            OrganizationId = organizationId,
            ClientId = clientId,
            InsurerId = insurerId,
            PolicyNumber = number,
            PolicyType = PolicyType.Property,
            StartDate = start,
            ExpiryDate = expiry,
            Premium = premium,
            SumInsured = premium * 10,
            CommissionPercentage = 10m,
            AssignedUserId = assignedUserId,
            Status = PolicyStatus.Active,
            VehicleNumber = number == "POL-A-NEAR" ? "MH-01-AB-4321" : null
        };
        PolicyFinancials.ApplyCommission(policy);
        return policy;
    }
}
