using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BrokerOS.Application.Auth;
using BrokerOS.Application.Common;

namespace BrokerOS.Api.Tests;

internal static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}

internal static class TestUsers
{
    public const string Password = "Demo@12345";

    public const string AdminA = "admin.a@brokeros.test";
    public const string ManagerA = "manager.a@brokeros.test";
    public const string EmployeeA = "employee.a@brokeros.test";
    public const string Employee2A = "employee2.a@brokeros.test";
    public const string AdminB = "admin.b@brokeros.test";
}

internal static class HttpClientAuth
{
    public static async Task<HttpClient> LoginAsAsync(this BrokerOsApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = TestUsers.Password },
            ApiJson.Options);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>(ApiJson.Options);
        Assert.NotNull(body?.Data?.AccessToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Data.AccessToken);
        return client;
    }

    public static async Task<ApiResponse<T>?> ReadApiAsync<T>(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ApiResponse<T>>(ApiJson.Options);
    }
}
