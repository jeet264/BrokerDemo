using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;

namespace BrokerOS.Application.Tests;

public sealed class AssignmentScopeTests
{
    [Fact]
    public void Employee_only_sees_assigned_clients()
    {
        var employee = User(role: UserRole.BrokerEmployee, userId: 4);
        var clients = new[]
        {
            new Client { Id = 1, AssignedUserId = 4, CompanyName = "Assigned" },
            new Client { Id = 2, AssignedUserId = 9, CompanyName = "Other book" },
            new Client { Id = 3, AssignedUserId = null, CompanyName = "Unassigned" }
        }.AsQueryable();

        var visible = clients.ForCurrentUser(employee).Select(client => client.CompanyName).ToList();

        Assert.Equal(new[] { "Assigned" }, visible);
    }

    [Theory]
    [InlineData(UserRole.BrokerAdmin)]
    [InlineData(UserRole.BrokerManager)]
    public void Admin_and_manager_see_the_whole_organisation_book(UserRole role)
    {
        var actor = User(role, userId: 1);
        var policies = new[]
        {
            new Policy { Id = 1, AssignedUserId = 1 },
            new Policy { Id = 2, AssignedUserId = 4 }
        }.AsQueryable();

        Assert.Equal(2, policies.ForCurrentUser(actor).Count());
    }

    [Fact]
    public void Employee_renewals_and_tasks_are_assignment_scoped()
    {
        var employee = User(UserRole.BrokerEmployee, userId: 4);
        var renewals = new[]
        {
            new Renewal { Id = 1, AssignedUserId = 4 },
            new Renewal { Id = 2, AssignedUserId = 8 }
        }.AsQueryable();
        var tasks = new[]
        {
            new WorkTask { Id = 1, AssignedUserId = 4 },
            new WorkTask { Id = 2, AssignedUserId = 8 }
        }.AsQueryable();

        Assert.Single(renewals.ForCurrentUser(employee));
        Assert.Single(tasks.ForCurrentUser(employee));
    }

    [Fact]
    public void EnsureCanAccessAssigned_hides_another_employees_record()
    {
        var employee = User(UserRole.BrokerEmployee, userId: 4);

        var error = Assert.Throws<NotFoundException>(() =>
            AssignmentScope.EnsureCanAccessAssigned(employee, assignedUserId: 9));

        Assert.Equal("The requested resource was not found.", error.Message);
    }

    [Fact]
    public void EnsureCanAccessAssigned_allows_employee_own_assignment()
    {
        var employee = User(UserRole.BrokerEmployee, userId: 4);
        AssignmentScope.EnsureCanAccessAssigned(employee, assignedUserId: 4);
    }

    [Fact]
    public void EnsureFound_throws_for_missing_tenant_rows()
    {
        Client? missing = null;
        Assert.Throws<NotFoundException>(() => AssignmentScope.EnsureFound(missing));
    }

    private static FakeCurrentUser User(UserRole role, long userId) =>
        new()
        {
            UserId = userId,
            PublicUserId = Guid.NewGuid(),
            OrganizationId = 1,
            Role = role,
            Email = $"{role.ToString().ToLowerInvariant()}@brokeros.test",
            IsAuthenticated = true
        };

    private sealed class FakeCurrentUser : ICurrentUserService
    {
        public long UserId { get; init; }

        public Guid PublicUserId { get; init; }

        public long OrganizationId { get; init; }

        public UserRole Role { get; init; }

        public string? Email { get; init; }

        public bool IsAuthenticated { get; init; }
    }
}
