using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cls.Tests.Infrastructure;

public static class ApiTestAuth
{
    public const string AdminEmail = "admin@cls.local";
    public const string AdminPassword = "Admin@2026!";
    public const string ManagerEmail = "manager@cls.local";
    public const string ManagerPassword = "Manager@2026!";
    public const string EmployeeEmail = "employee@cls.local";
    public const string EmployeePassword = "Employee@2026!";

    public static Task<string> LoginAdminAsync(HttpClient client) =>
        LoginAsync(client, AdminEmail, AdminPassword);

    public static Task<string> LoginManagerAsync(HttpClient client) =>
        LoginAsync(client, ManagerEmail, ManagerPassword);

    public static Task<string> LoginEmployeeAsync(HttpClient client) =>
        LoginAsync(client, EmployeeEmail, EmployeePassword);

    public static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    public static Task<HttpResponseMessage> GetAsync(HttpClient client, string token, string url) =>
        SendAsync(client, HttpMethod.Get, token, url);

    public static Task<HttpResponseMessage> PostAsync(HttpClient client, string token, string url, HttpContent? content = null) =>
        SendAsync(client, HttpMethod.Post, token, url, content);

    public static Task<HttpResponseMessage> PostJsonAsync<T>(HttpClient client, string token, string url, T body) =>
        SendAsync(client, HttpMethod.Post, token, url, JsonContent.Create(body));

    public static async Task<int> GetCurrentUserIdAsync(HttpClient client, string token)
    {
        var response = await GetAsync(client, token, "/api/v1/general/me");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    private static Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        HttpMethod method,
        string token,
        string url,
        HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client.SendAsync(request);
    }
}
