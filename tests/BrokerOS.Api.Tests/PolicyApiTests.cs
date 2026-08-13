using System.Net;
using System.Net.Http.Json;
using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;
using BrokerOS.Application.Renewals;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class PolicyApiTests
{
    private readonly BrokerOsApiFactory _factory;

    public PolicyApiTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_lists_policies_in_the_organisation()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.GetAsync("/api/policies?pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<PagedResult<PolicyListDto>>();
        Assert.Contains(body!.Data!.Items, item => item.PublicId == _factory.Catalog.OrgAPolicyNearPublicId);
        Assert.DoesNotContain(body.Data.Items, item => item.PublicId == _factory.Catalog.OrgBPolicyPublicId);
    }

    [Fact]
    public async Task Admin_gets_policy_details_with_computed_commission()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.GetAsync($"/api/policies/{_factory.Catalog.OrgAPolicyNearPublicId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<PolicyDetailsDto>();
        Assert.Equal("POL-A-NEAR", body!.Data!.PolicyNumber);
        Assert.Equal(10_000.00m, body.Data.CommissionAmount);
        Assert.Equal(_factory.Catalog.NearPremium, body.Data.Premium);
    }

    [Fact]
    public async Task Creating_a_policy_computes_commission_and_creates_a_renewal()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);
        var policyNumber = $"POL-NEW-{Guid.NewGuid():N}"[..20];
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var expiry = start.AddYears(1);

        var create = await client.PostAsJsonAsync("/api/policies", new
        {
            policyNumber,
            clientPublicId = _factory.Catalog.OrgAClientAssignedPublicId,
            insurerPublicId = _factory.Catalog.OrgAInsurerPublicId,
            policyType = "Marine",
            startDate = start,
            expiryDate = expiry,
            premium = 200_000m,
            sumInsured = 2_000_000m,
            commissionPercentage = 12.5m,
            assignedUserPublicId = _factory.Catalog.OrgAEmployeePublicId
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var policy = await create.ReadApiAsync<PolicyDetailsDto>();
        Assert.Equal(25_000.00m, policy!.Data!.CommissionAmount);
        Assert.Equal(policyNumber, policy.Data.PolicyNumber);
        Assert.NotNull(policy.Data.RenewalPublicId);

        var renewals = await client.GetAsync($"/api/renewals?search={policyNumber}&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, renewals.StatusCode);
        var renewalPage = await renewals.ReadApiAsync<PagedResult<RenewalListDto>>();
        var renewal = Assert.Single(renewalPage!.Data!.Items);
        Assert.Equal(policy.Data.PublicId, renewal.PolicyPublicId);
        Assert.Equal("Upcoming", renewal.Status);
        Assert.Equal(expiry, renewal.RenewalDate);
    }
}
