using System.Net;
using System.Net.Http.Json;
using BrokerOS.Application.Auth;
using BrokerOS.Application.Common;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class AuthenticationTests
{
    private readonly BrokerOsApiFactory _factory;

    public AuthenticationTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_with_valid_password_returns_token_and_user()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = TestUsers.AdminA, password = TestUsers.Password },
            ApiJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<AuthResponseDto>();
        Assert.True(body!.Success);
        Assert.False(string.IsNullOrWhiteSpace(body.Data!.AccessToken));
        Assert.Equal("BrokerAdmin", body.Data.User.Role);
        Assert.Equal(TestUsers.AdminA, body.Data.User.Email);
    }

    [Fact]
    public async Task Login_with_invalid_password_is_unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = TestUsers.AdminA, password = "WrongPassword!1" },
            ApiJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.ReadApiAsync<object>();
        Assert.False(body!.Success);
    }

    [Fact]
    public async Task Anonymous_requests_require_authentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/clients");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_the_authenticated_user()
    {
        var client = await _factory.LoginAsAsync(TestUsers.ManagerA);

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadApiAsync<CurrentUserDto>();
        Assert.Equal(TestUsers.ManagerA, body!.Data!.Email);
        Assert.Equal("BrokerManager", body.Data.Role);
    }
}
