using System.Net;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class CrossTenantTests
{
    private readonly BrokerOsApiFactory _factory;

    public CrossTenantTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Org_B_cannot_read_org_A_client()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminB);

        var response = await client.GetAsync($"/api/clients/{_factory.Catalog.OrgAClientAssignedPublicId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Org_B_cannot_read_org_A_policy()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminB);

        var response = await client.GetAsync($"/api/policies/{_factory.Catalog.OrgAPolicyNearPublicId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Org_B_cannot_read_org_A_renewal()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminB);

        var response = await client.GetAsync($"/api/renewals/{_factory.Catalog.OrgARenewalNearPublicId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Org_B_cannot_read_org_A_dashboard_figures()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminB);

        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<BrokerOS.Application.Dashboard.DashboardDto>();
        Assert.DoesNotContain(
            body!.Data!.UpcomingRenewals,
            item => item.PolicyNumber == "POL-A-NEAR");
        Assert.Equal(50_000m, body.Data.PremiumAtRisk);
    }
}
