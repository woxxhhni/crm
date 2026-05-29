using System.Net.Http.Json;
using System.Text.Json;

namespace Cls.Tests.Infrastructure;

public static class ApiTestDataBuilder
{
    public static async Task<int> CreateClientAsync(HttpClient client, string adminToken)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent($"Test Client {Guid.NewGuid():N}"), "Name" },
            { new StringContent("true"), "IsActive" }
        };

        var response = await ApiTestAuth.PostAsync(client, adminToken, "/api/v1/clients", form);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    public static async Task<int> CreateProviderAsync(HttpClient client, string adminToken)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent($"Test Provider {Guid.NewGuid():N}"), "Name" },
            { new StringContent("true"), "IsActive" }
        };

        var response = await ApiTestAuth.PostAsync(client, adminToken, "/api/v1/providers", form);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    public static async Task<int> CreateOrderAsync(
        HttpClient client,
        string adminToken,
        int clientId,
        int providerId,
        params int[] assignedEmployeeIds)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent($"Regression Order {Guid.NewGuid():N}"), "Title" },
            { new StringContent(DateTime.UtcNow.ToString("o")), "OrderDate" },
            { new StringContent("USD"), "BuyCurrency" },
            { new StringContent("100"), "BuyAmount" },
            { new StringContent("EUR"), "SellCurrency" },
            { new StringContent("90"), "SellAmount" },
            { new StringContent(clientId.ToString()), "ClientId" },
            { new StringContent(providerId.ToString()), "ProviderId" }
        };

        for (var i = 0; i < assignedEmployeeIds.Length; i++)
            form.Add(new StringContent(assignedEmployeeIds[i].ToString()), $"employees.UserIds[{i}]");

        var response = await ApiTestAuth.PostAsync(client, adminToken, "/api/v1/orders", form);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    public static async Task<(int UserId, string Email, string Password)> CreateEmployeeUserAsync(
        HttpClient client,
        string adminToken)
    {
        const string password = "Employee@2026!";
        var email = $"employee.{Guid.NewGuid():N}@cls.local";

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Regression Employee"), "Name" },
            { new StringContent(email), "Email" },
            { new StringContent(password), "Password" },
            { new StringContent("Employee"), "Role" },
            { new StringContent("true"), "IsActive" }
        };

        var response = await ApiTestAuth.PostAsync(client, adminToken, "/api/v1/employees", form);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (body.GetProperty("id").GetInt32(), email, password);
    }
}
