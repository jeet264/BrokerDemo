using System.Net;
using System.Net.Http.Json;
using BrokerOS.Application.Common;
using BrokerOS.Application.Renewals;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class RenewalApiTests
{
    private readonly BrokerOsApiFactory _factory;

    public RenewalApiTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_lists_and_gets_renewals()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var list = await client.GetAsync("/api/renewals?pageSize=50");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var page = await list.ReadApiAsync<PagedResult<RenewalListDto>>();
        Assert.Contains(page!.Data!.Items, item => item.PublicId == _factory.Catalog.OrgARenewalNearPublicId);
        Assert.DoesNotContain(page.Data.Items, item => item.PublicId == _factory.Catalog.OrgBRenewalPublicId);

        var detail = await client.GetAsync($"/api/renewals/{_factory.Catalog.OrgARenewalNearPublicId}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var body = await detail.ReadApiAsync<RenewalDetailsDto>();
        Assert.Equal("POL-A-NEAR", body!.Data!.PolicyNumber);
        Assert.True(body.Data.DaysRemaining > 0);
    }

    [Fact]
    public async Task Assigned_employee_can_update_status_and_stage()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);
        var publicId = _factory.Catalog.OrgARenewalNearPublicId;

        var status = await client.PutAsJsonAsync($"/api/renewals/{publicId}/status", new
        {
            status = "InProgress",
            notes = "Working the file"
        }, ApiJson.Options);
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        var afterStatus = await status.ReadApiAsync<RenewalDetailsDto>();
        Assert.Equal("InProgress", afterStatus!.Data!.Status);

        var stage = await client.PutAsJsonAsync($"/api/renewals/{publicId}/stage", new
        {
            stage = "ClientContact",
            notes = "Spoke to the client"
        }, ApiJson.Options);
        Assert.Equal(HttpStatusCode.OK, stage.StatusCode);
        var afterStage = await stage.ReadApiAsync<RenewalDetailsDto>();
        Assert.Equal("ClientContact", afterStage!.Data!.CurrentStage);
    }

    [Fact]
    public async Task Assigned_employee_can_create_a_follow_up_task()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);
        var due = DateTime.UtcNow.AddDays(3);

        var response = await client.PostAsJsonAsync(
            $"/api/renewals/{_factory.Catalog.OrgARenewalNearPublicId}/tasks",
            new
            {
                title = "Send quotation pack",
                description = "Attach last year's wording.",
                dueDateUtc = due,
                priority = "High"
            },
            ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<RenewalDetailsDto>();
        Assert.Contains(body!.Data!.Activities, activity => activity.Description.Contains("Send quotation pack"));
    }
}
