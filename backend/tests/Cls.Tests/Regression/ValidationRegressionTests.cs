using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cls.Tests.Infrastructure;
using FluentAssertions;

namespace Cls.Tests.Regression;

public class ValidationRegressionTests(ClsWebApplicationFactory factory) : IClassFixture<ClsWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateOrder_WithInvalidAmount_Returns422WithErrorShape()
    {
        var adminToken = await ApiTestAuth.LoginAdminAsync(_client);
        var clientId = await ApiTestDataBuilder.CreateClientAsync(_client, adminToken);
        var providerId = await ApiTestDataBuilder.CreateProviderAsync(_client, adminToken);

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Invalid Amount Order"), "Title" },
            { new StringContent(DateTime.UtcNow.ToString("o")), "OrderDate" },
            { new StringContent("USD"), "BuyCurrency" },
            { new StringContent("0"), "BuyAmount" },
            { new StringContent("EUR"), "SellCurrency" },
            { new StringContent("90"), "SellAmount" },
            { new StringContent(clientId.ToString()), "ClientId" },
            { new StringContent(providerId.ToString()), "ProviderId" }
        };

        var response = await ApiTestAuth.PostAsync(_client, adminToken, "/api/v1/orders", form);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetInt32().Should().Be(422);
        body.GetProperty("detail").GetString().Should().Contain("Buy Amount");
    }
}
