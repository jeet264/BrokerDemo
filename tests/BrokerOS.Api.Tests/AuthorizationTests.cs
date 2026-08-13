using System.Net;
using System.Net.Http.Json;
using BrokerOS.Application.Clients;
using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class AuthorizationTests
{
    private readonly BrokerOsApiFactory _factory;

    public AuthorizationTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Employee_cannot_create_clients()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);

        var response = await client.PostAsJsonAsync("/api/clients", ValidClient("employee-blocked"), ApiJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_create_policies()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var response = await client.PostAsJsonAsync("/api/policies", new
        {
            policyNumber = $"POL-EMP-{Guid.NewGuid():N}"[..18],
            clientPublicId = _factory.Catalog.OrgAClientAssignedPublicId,
            insurerPublicId = _factory.Catalog.OrgAInsurerPublicId,
            policyType = "Property",
            startDate = start,
            expiryDate = start.AddYears(1),
            premium = 10_000m,
            sumInsured = 100_000m,
            commissionPercentage = 10m
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_can_create_clients()
    {
        var client = await _factory.LoginAsAsync(TestUsers.ManagerA);

        var response = await client.PostAsJsonAsync("/api/clients", ValidClient("manager-ok"), ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<ClientDetailsDto>();
        Assert.Contains("manager-ok", body!.Data!.CompanyName);
    }

    [Fact]
    public async Task Admin_can_create_clients()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.PostAsJsonAsync("/api/clients", ValidClient("admin-ok"), ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_sees_only_assigned_clients_and_gets_404_for_others()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);

        var list = await client.GetAsync("/api/clients?pageSize=50");
        var page = await list.ReadApiAsync<PagedResult<ClientListDto>>();
        Assert.Contains(page!.Data!.Items, item => item.PublicId == _factory.Catalog.OrgAClientAssignedPublicId);
        Assert.DoesNotContain(page.Data.Items, item => item.PublicId == _factory.Catalog.OrgAClientOtherPublicId);

        var hidden = await client.GetAsync($"/api/clients/{_factory.Catalog.OrgAClientOtherPublicId}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);

        var assigned = await client.GetAsync($"/api/clients/{_factory.Catalog.OrgAClientAssignedPublicId}");
        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);
    }

    [Fact]
    public async Task Manager_can_see_another_employees_client()
    {
        var client = await _factory.LoginAsAsync(TestUsers.ManagerA);

        var response = await client.GetAsync($"/api/clients/{_factory.Catalog.OrgAClientOtherPublicId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_read_unassigned_policy()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);

        var response = await client.GetAsync($"/api/policies/{_factory.Catalog.OrgAPolicyFarPublicId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static object ValidClient(string marker)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new
        {
            companyName = $"{marker} {suffix}",
            clientType = "SME",
            email = $"{marker}-{suffix}@brokeros.test",
            phone = "+91 90000 22222",
            addressLine1 = "9 Role Street",
            city = "Bengaluru",
            state = "Karnataka",
            postalCode = "560001",
            country = "India"
        };
    }
}
