using System.Net;
using System.Net.Http.Json;
using BrokerOS.Application.Common;
using BrokerOS.Application.Notifications;
using BrokerOS.Application.Policies;
using BrokerOS.Application.Quotations;
using BrokerOS.Application.Renewals;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class QuotationApiTests
{
    private readonly BrokerOsApiFactory _factory;

    public QuotationApiTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_logs_selects_and_shares_quotations_on_a_renewal()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);
        var renewalId = _factory.Catalog.OrgARenewalNearPublicId;

        var empty = await client.GetAsync($"/api/renewals/{renewalId}/quotations");
        Assert.Equal(HttpStatusCode.OK, empty.StatusCode);
        var emptyBody = await empty.ReadApiAsync<IReadOnlyList<QuotationDto>>();
        Assert.NotNull(emptyBody!.Data);

        var first = await client.PostAsJsonAsync($"/api/renewals/{renewalId}/quotations", new
        {
            insurerPublicId = _factory.Catalog.OrgAInsurerPublicId,
            premiumAmount = 110_000m,
            sumInsured = 1_200_000m,
            coverageSummary = "As expiring, NCB retained"
        }, ApiJson.Options);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstQuote = await first.ReadApiAsync<QuotationDto>();
        Assert.Equal("Received", firstQuote!.Data!.Status);
        Assert.Equal("Test New India", firstQuote.Data.InsurerName);

        var second = await client.PostAsJsonAsync($"/api/renewals/{renewalId}/quotations", new
        {
            newInsurerName = "Tata AIG",
            premiumAmount = 95_000m,
            coverageSummary = "Wider cover, slightly cheaper"
        }, ApiJson.Options);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondQuote = await second.ReadApiAsync<QuotationDto>();
        Assert.Equal("Tata AIG", secondQuote!.Data!.InsurerName);
        Assert.True(secondQuote.Data.IsLowestPremium);

        var select = await client.PutAsJsonAsync(
            $"/api/quotations/{secondQuote.Data.PublicId}/select",
            new { },
            ApiJson.Options);
        Assert.Equal(HttpStatusCode.OK, select.StatusCode);
        var selected = await select.ReadApiAsync<QuotationDto>();
        Assert.Equal("Selected", selected!.Data!.Status);

        var list = await client.GetAsync($"/api/renewals/{renewalId}/quotations");
        var listed = await list.ReadApiAsync<IReadOnlyList<QuotationDto>>();
        var quotes = listed!.Data!;
        Assert.Equal("Selected", quotes.Single(item => item.PublicId == secondQuote.Data!.PublicId).Status);
        Assert.Equal("Rejected", quotes.Single(item => item.PublicId == firstQuote.Data!.PublicId).Status);

        var detail = await client.GetAsync($"/api/renewals/{renewalId}");
        var renewal = await detail.ReadApiAsync<RenewalDetailsDto>();
        Assert.Equal("Tata AIG", renewal!.Data!.SelectedQuotation!.InsurerName);
        Assert.Equal(95_000m, renewal.Data.SelectedQuotation.PremiumAmount);

        var share = await client.PostAsync($"/api/quotations/{secondQuote.Data.PublicId}/share", null);
        Assert.Equal(HttpStatusCode.OK, share.StatusCode);
        var shared = await share.ReadApiAsync<NotificationDto>();
        Assert.Equal("WhatsApp", shared!.Data!.Channel);
        Assert.Equal("Client", shared.Data.RecipientType);
        Assert.Equal("Simulated", shared.Data.Status);
        Assert.Contains("Tata AIG", shared.Data.Body);
        Assert.Contains("Hi ", shared.Data.Body);

        var compare = await client.PostAsync($"/api/renewals/{renewalId}/quotations/compare-share", null);
        Assert.Equal(HttpStatusCode.OK, compare.StatusCode);
        var comparison = await compare.ReadApiAsync<NotificationDto>();
        Assert.Contains("lowest", comparison!.Data!.Body);
        Assert.Contains("selected", comparison.Data.Body);
    }

    [Fact]
    public async Task Mark_renewed_prefills_from_the_selected_quotation()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);
        var policyNumber = $"POL-Q-{Guid.NewGuid():N}"[..20];
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-300);
        var expiry = start.AddYears(1);

        var createPolicy = await client.PostAsJsonAsync("/api/policies", new
        {
            policyNumber,
            clientPublicId = _factory.Catalog.OrgAClientAssignedPublicId,
            insurerPublicId = _factory.Catalog.OrgAInsurerPublicId,
            policyType = "Property",
            startDate = start,
            expiryDate = expiry,
            premium = 200_000m,
            sumInsured = 2_000_000m,
            commissionPercentage = 10m
        }, ApiJson.Options);
        Assert.Equal(HttpStatusCode.OK, createPolicy.StatusCode);
        var policy = await createPolicy.ReadApiAsync<PolicyDetailsDto>();
        var renewalId = policy!.Data!.RenewalPublicId!.Value;

        var quote = await client.PostAsJsonAsync($"/api/renewals/{renewalId}/quotations", new
        {
            newInsurerName = "ICICI Lombard",
            premiumAmount = 175_000m,
            sumInsured = 2_200_000m,
            coverageSummary = "Same cover, lower premium"
        }, ApiJson.Options);
        var quotation = await quote.ReadApiAsync<QuotationDto>();
        await client.PutAsJsonAsync($"/api/quotations/{quotation!.Data!.PublicId}/select", new { }, ApiJson.Options);

        var complete = await client.PutAsJsonAsync($"/api/renewals/{renewalId}/complete", new { }, ApiJson.Options);
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
        var renewed = await complete.ReadApiAsync<RenewalDetailsDto>();
        Assert.Equal("Renewed", renewed!.Data!.Status);
        Assert.NotNull(renewed.Data.NextPolicyPublicId);

        var next = await client.GetAsync($"/api/policies/{renewed.Data.NextPolicyPublicId}");
        var nextPolicy = await next.ReadApiAsync<PolicyDetailsDto>();
        Assert.Equal(175_000m, nextPolicy!.Data!.Premium);
        Assert.Equal(2_200_000m, nextPolicy.Data.SumInsured);
        Assert.Equal("ICICI Lombard", nextPolicy.Data.InsurerName);
        Assert.Equal(quotation.Data.InsurerPublicId, nextPolicy.Data.InsurerPublicId);
    }

    [Fact]
    public async Task Other_org_renewal_quotations_are_not_found()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminB);

        var response = await client.GetAsync(
            $"/api/renewals/{_factory.Catalog.OrgARenewalNearPublicId}/quotations");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_quote_an_unassigned_renewal()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);
        var renewals = await client.GetAsync("/api/renewals?search=POL-A-FAR&pageSize=20");
        var page = await renewals.ReadApiAsync<PagedResult<RenewalListDto>>();
        Assert.Empty(page!.Data!.Items);

        var far = await _factory.LoginAsAsync(TestUsers.AdminA);
        var farList = await far.GetAsync("/api/renewals?search=POL-A-FAR&pageSize=20");
        var farPage = await farList.ReadApiAsync<PagedResult<RenewalListDto>>();
        var farId = Assert.Single(farPage!.Data!.Items).PublicId;

        var response = await client.PostAsJsonAsync($"/api/renewals/{farId}/quotations", new
        {
            insurerPublicId = _factory.Catalog.OrgAInsurerPublicId,
            premiumAmount = 1m
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Missing_insurer_is_validation_error()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.PostAsJsonAsync(
            $"/api/renewals/{_factory.Catalog.OrgARenewalNearPublicId}/quotations",
            new { premiumAmount = 10m },
            ApiJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_is_unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/renewals/{_factory.Catalog.OrgARenewalNearPublicId}/quotations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
