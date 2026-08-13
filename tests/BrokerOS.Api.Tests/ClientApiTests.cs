using System.Net;
using System.Net.Http.Json;
using BrokerOS.Application.Clients;
using BrokerOS.Application.Common;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class ClientApiTests
{
    private readonly BrokerOsApiFactory _factory;

    public ClientApiTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_lists_organisation_clients()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.GetAsync("/api/clients?pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<PagedResult<ClientListDto>>();
        Assert.True(body!.Success);
        Assert.Contains(body.Data!.Items, item => item.PublicId == _factory.Catalog.OrgAClientAssignedPublicId);
        Assert.Contains(body.Data.Items, item => item.PublicId == _factory.Catalog.OrgAClientOtherPublicId);
        Assert.DoesNotContain(body.Data.Items, item => item.PublicId == _factory.Catalog.OrgBClientPublicId);
    }

    [Fact]
    public async Task Admin_gets_client_details()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.GetAsync($"/api/clients/{_factory.Catalog.OrgAClientAssignedPublicId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<ClientDetailsDto>();
        Assert.Equal("Alpha Logistics", body!.Data!.CompanyName);
    }

    [Fact]
    public async Task Admin_creates_a_client()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var response = await client.PostAsJsonAsync("/api/clients", new
        {
            companyName = $"New Client {suffix}",
            clientType = "Corporate",
            industry = "Manufacturing",
            email = $"new-{suffix}@brokeros.test",
            phone = "+91 90000 11111",
            addressLine1 = "2 Broker Lane",
            city = "Pune",
            state = "Maharashtra",
            postalCode = "411001",
            country = "India"
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<ClientDetailsDto>();
        Assert.Equal($"New Client {suffix}", body!.Data!.CompanyName);
        Assert.False(string.IsNullOrWhiteSpace(body.Data.ClientCode));
    }

    [Fact]
    public async Task Missing_company_name_is_validation_error()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.PostAsJsonAsync("/api/clients", new
        {
            companyName = "",
            clientType = "Corporate",
            email = "bad@brokeros.test",
            phone = "+91 90000 11111",
            addressLine1 = "2 Broker Lane",
            city = "Pune",
            state = "Maharashtra",
            postalCode = "411001",
            country = "India"
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
