using System.Net;
using BrokerOS.Application.Common;
using BrokerOS.Application.Search;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class SearchApiTests
{
    private readonly BrokerOsApiFactory _factory;

    public SearchApiTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_finds_client_by_name_and_phone()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var byName = await client.GetAsync("/api/search?q=Alpha");
        Assert.Equal(HttpStatusCode.OK, byName.StatusCode);
        var nameBody = await byName.ReadApiAsync<SearchResultsDto>();
        Assert.Contains(
            nameBody!.Data!.Items,
            item => item.Type == "Client" && item.PublicId == _factory.Catalog.OrgAClientAssignedPublicId);

        var byPhone = await client.GetAsync("/api/search?q=90000");
        var phoneBody = await byPhone.ReadApiAsync<SearchResultsDto>();
        Assert.Contains(
            phoneBody!.Data!.Items,
            item => item.Type == "Client" && item.PublicId == _factory.Catalog.OrgAClientAssignedPublicId);
    }

    [Fact]
    public async Task Admin_finds_policy_by_number_and_vehicle()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var byNumber = await client.GetAsync("/api/search?q=POL-A-NEAR");
        var numberBody = await byNumber.ReadApiAsync<SearchResultsDto>();
        var policyHit = Assert.Single(numberBody!.Data!.Items, item => item.Type == "Policy");
        Assert.Equal(_factory.Catalog.OrgAPolicyNearPublicId, policyHit.PublicId);
        Assert.Equal("PolicyNumber", policyHit.MatchedOn);

        var byVehicle = await client.GetAsync("/api/search?q=MH01AB4321");
        var vehicleBody = await byVehicle.ReadApiAsync<SearchResultsDto>();
        Assert.Contains(
            vehicleBody!.Data!.Items,
            item => item.Type == "Policy" && item.PublicId == _factory.Catalog.OrgAPolicyNearPublicId);
    }

    [Fact]
    public async Task Short_query_returns_empty()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.GetAsync("/api/search?q=A");
        var body = await response.ReadApiAsync<SearchResultsDto>();
        Assert.Empty(body!.Data!.Items);
    }

    [Fact]
    public async Task Other_org_does_not_see_org_A_hits()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminB);

        var response = await client.GetAsync("/api/search?q=Alpha");
        var body = await response.ReadApiAsync<SearchResultsDto>();
        Assert.DoesNotContain(
            body!.Data!.Items,
            item => item.PublicId == _factory.Catalog.OrgAClientAssignedPublicId);
    }

    [Fact]
    public async Task Employee_does_not_see_unassigned_client()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);

        var response = await client.GetAsync("/api/search?q=Beta");
        var body = await response.ReadApiAsync<SearchResultsDto>();
        Assert.DoesNotContain(
            body!.Data!.Items,
            item => item.PublicId == _factory.Catalog.OrgAClientOtherPublicId);
    }
}
