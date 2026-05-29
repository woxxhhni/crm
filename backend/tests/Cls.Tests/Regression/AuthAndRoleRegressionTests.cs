using System.Net;
using Cls.Tests.Infrastructure;
using FluentAssertions;

namespace Cls.Tests.Regression;

public class AuthAndRoleRegressionTests(ClsWebApplicationFactory factory) : IClassFixture<ClsWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/orders?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OrderSummary_WhenEmployee_Returns403()
    {
        var token = await ApiTestAuth.LoginEmployeeAsync(_client);
        var response = await ApiTestAuth.GetAsync(_client, token, "/api/v1/orders/summary");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OrderSummary_WhenManager_Returns200()
    {
        var token = await ApiTestAuth.LoginManagerAsync(_client);
        var response = await ApiTestAuth.GetAsync(_client, token, "/api/v1/orders/summary");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListBackups_WhenEmployee_Returns403()
    {
        var token = await ApiTestAuth.LoginEmployeeAsync(_client);
        var response = await ApiTestAuth.GetAsync(_client, token, "/api/v1/backups");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListBackups_WhenManager_Returns403()
    {
        var token = await ApiTestAuth.LoginManagerAsync(_client);
        var response = await ApiTestAuth.GetAsync(_client, token, "/api/v1/backups");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
