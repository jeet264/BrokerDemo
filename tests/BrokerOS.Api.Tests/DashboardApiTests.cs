using System.Net;
using BrokerOS.Application.Common;
using BrokerOS.Application.Dashboard;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class DashboardApiTests
{
    private readonly BrokerOsApiFactory _factory;

    public DashboardApiTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_dashboard_includes_org_totals_and_premium_at_risk()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<DashboardDto>();
        Assert.True(body!.Success);
        Assert.True(body.Data!.TotalClients >= 2);
        Assert.True(body.Data.ActivePolicies >= 2);
        Assert.Equal(_factory.Catalog.NearPremium, body.Data.PremiumAtRisk);
        Assert.Contains(
            body.Data.UpcomingRenewals,
            item => item.PolicyNumber == "POL-A-NEAR");
        Assert.DoesNotContain(
            body.Data.UpcomingRenewals,
            item => item.PolicyNumber == "POL-B-NEAR");
    }

    [Fact]
    public async Task Employee_dashboard_is_limited_to_assigned_book()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);

        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<DashboardDto>();
        Assert.Equal(1, body!.Data!.TotalClients);
        Assert.True(body.Data.ActivePolicies >= 1);
        Assert.Equal(_factory.Catalog.NearPremium, body.Data.PremiumAtRisk);
        Assert.DoesNotContain(
            body.Data.UpcomingRenewals,
            item => item.PolicyNumber == "POL-A-FAR");
    }
}
