using System.Net;
using System.Net.Http.Json;
using BrokerOS.Application.Clients;
using BrokerOS.Application.Common;
using BrokerOS.Application.QuickNotes;
using BrokerOS.Application.Tasks;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class QuickNoteApiTests
{
    private readonly BrokerOsApiFactory _factory;

    public QuickNoteApiTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_saves_an_unlinked_note()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.PostAsJsonAsync("/api/quick-notes", new
        {
            text = "Callback after lunch — name not in the book yet."
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<QuickNoteDto>();
        Assert.True(body!.Success);
        Assert.Equal("Callback after lunch — name not in the book yet.", body.Data!.Text);
        Assert.False(body.Data.FollowUpTaskCreated);
        Assert.Null(body.Data.ClientPublicId);
        Assert.Null(body.Data.TaskPublicId);
    }

    [Fact]
    public async Task Admin_saves_a_client_note_and_follow_up_task()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);
        var due = DateTime.UtcNow.AddDays(2);

        var response = await client.PostAsJsonAsync("/api/quick-notes", new
        {
            text = "Alpha wants the quote by Friday.",
            clientPublicId = _factory.Catalog.OrgAClientAssignedPublicId,
            createFollowUpTask = true,
            taskDueDateUtc = due
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<QuickNoteDto>();
        Assert.True(body!.Data!.FollowUpTaskCreated);
        Assert.Equal(_factory.Catalog.OrgAClientAssignedPublicId, body.Data.ClientPublicId);
        Assert.NotNull(body.Data.TaskPublicId);

        var activities = await client.GetAsync($"/api/clients/{_factory.Catalog.OrgAClientAssignedPublicId}/activities");
        var activityPage = await activities.ReadApiAsync<IReadOnlyList<ClientActivityDto>>();
        Assert.Contains(activityPage!.Data!, item => item.Description.Contains("quote by Friday"));

        var tasks = await client.GetAsync("/api/tasks?search=quote%20by%20Friday&pageSize=20");
        var taskPage = await tasks.ReadApiAsync<PagedResult<TaskListDto>>();
        Assert.Contains(taskPage!.Data!.Items, item => item.PublicId == body.Data.TaskPublicId);
    }

    [Fact]
    public async Task Renewal_note_links_client_from_the_file()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.PostAsJsonAsync("/api/quick-notes", new
        {
            text = "Spoke on WhatsApp — send comparison tomorrow.",
            renewalPublicId = _factory.Catalog.OrgARenewalNearPublicId
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<QuickNoteDto>();
        Assert.Equal(_factory.Catalog.OrgARenewalNearPublicId, body!.Data!.RenewalPublicId);
        Assert.Equal(_factory.Catalog.OrgAClientAssignedPublicId, body.Data.ClientPublicId);
        Assert.Equal("POL-A-NEAR", body.Data.PolicyNumber);
    }

    [Fact]
    public async Task Empty_text_is_validation_error()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminA);

        var response = await client.PostAsJsonAsync("/api/quick-notes", new { text = "" }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Other_org_client_is_not_found()
    {
        var client = await _factory.LoginAsAsync(TestUsers.AdminB);

        var response = await client.PostAsJsonAsync("/api/quick-notes", new
        {
            text = "Should not land on Org A.",
            clientPublicId = _factory.Catalog.OrgAClientAssignedPublicId
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_link_an_unassigned_client()
    {
        var client = await _factory.LoginAsAsync(TestUsers.EmployeeA);

        var response = await client.PostAsJsonAsync("/api/quick-notes", new
        {
            text = "Tried to note on someone else's book.",
            clientPublicId = _factory.Catalog.OrgAClientOtherPublicId
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_is_unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/quick-notes", new { text = "No token" }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
