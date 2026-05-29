using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cls.Tests.Infrastructure;
using FluentAssertions;

namespace Cls.Tests.Regression;

public class OrderEmployeeAccessRegressionTests(ClsWebApplicationFactory factory) : IClassFixture<ClsWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AssignedEmployee_CanReadOrderAndAddNote()
    {
        var adminToken = await ApiTestAuth.LoginAdminAsync(_client);
        var employeeToken = await ApiTestAuth.LoginEmployeeAsync(_client);
        var employeeId = await ApiTestAuth.GetCurrentUserIdAsync(_client, employeeToken);

        var clientId = await ApiTestDataBuilder.CreateClientAsync(_client, adminToken);
        var providerId = await ApiTestDataBuilder.CreateProviderAsync(_client, adminToken);
        var orderId = await ApiTestDataBuilder.CreateOrderAsync(
            _client, adminToken, clientId, providerId, employeeId);

        var getOrder = await ApiTestAuth.GetAsync(_client, employeeToken, $"/api/v1/orders/{orderId}");
        getOrder.StatusCode.Should().Be(HttpStatusCode.OK);

        using var noteForm = new MultipartFormDataContent
        {
            { new StringContent(DateTime.UtcNow.ToString("o")), "ActionDate" },
            { new StringContent("Regression note"), "Title" },
            { new StringContent("Assigned employee can add notes"), "Description" }
        };

        var addNote = await ApiTestAuth.PostAsync(_client, employeeToken, $"/api/v1/orders/{orderId}/notes", noteForm);
        addNote.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnassignedEmployee_CannotReadOrderOrAddNote()
    {
        var adminToken = await ApiTestAuth.LoginAdminAsync(_client);
        var assignedEmployeeToken = await ApiTestAuth.LoginEmployeeAsync(_client);
        var assignedEmployeeId = await ApiTestAuth.GetCurrentUserIdAsync(_client, assignedEmployeeToken);

        var (unassignedEmployeeId, unassignedEmail, unassignedPassword) =
            await ApiTestDataBuilder.CreateEmployeeUserAsync(_client, adminToken);
        unassignedEmployeeId.Should().NotBe(assignedEmployeeId);

        var unassignedToken = await ApiTestAuth.LoginAsync(_client, unassignedEmail, unassignedPassword);

        var clientId = await ApiTestDataBuilder.CreateClientAsync(_client, adminToken);
        var providerId = await ApiTestDataBuilder.CreateProviderAsync(_client, adminToken);
        var orderId = await ApiTestDataBuilder.CreateOrderAsync(
            _client, adminToken, clientId, providerId, assignedEmployeeId);

        var getOrder = await ApiTestAuth.GetAsync(_client, unassignedToken, $"/api/v1/orders/{orderId}");
        getOrder.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var noteForm = new MultipartFormDataContent
        {
            { new StringContent(DateTime.UtcNow.ToString("o")), "ActionDate" },
            { new StringContent("Blocked note"), "Title" },
            { new StringContent("Should not be allowed"), "Description" }
        };

        var addNote = await ApiTestAuth.PostAsync(_client, unassignedToken, $"/api/v1/orders/{orderId}/notes", noteForm);
        addNote.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await addNote.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetInt32().Should().Be(403);
        body.GetProperty("detail").GetString().Should().Be("Invalid access");
    }
}
