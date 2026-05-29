using System.Text.Json;
using System.Text.Json.Serialization;
using Cls.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Cls.Infrastructure.Auth;

using System.Net.Http;
using System.Net.Http.Json;

public class RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IRecaptchaService
{
    private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    public async Task<bool> VerifyAsync(string? token)
    {
        var enabled = configuration.GetValue<bool>("Recaptcha:Enabled", true);

        if (!enabled)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("[Auth] Recaptcha token is null or whitespace");
            return false;
        }

        var secretKey = configuration["Recaptcha:SecretKey"];
        var threshold = configuration.GetValue<double>("Recaptcha:Threshold", 0.5);

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            Console.WriteLine("[Auth] Recaptcha SecretKey is not configured.");
            throw new InvalidOperationException("Recaptcha SecretKey is not configured.");
        }

        var client = httpClientFactory.CreateClient();
        Console.WriteLine($"[Auth] Verifying Recaptcha Token: {token.Substring(0, Math.Min(token.Length, 10))}...");
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret", secretKey },
            { "response", token }
        });

        var response = await client.PostAsync(VerifyUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[Auth] Recaptcha HTTP request failed with status code {response.StatusCode}.");
            return false;
        }

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[Auth] Recaptcha Verification Response: {json}");

        var result = JsonSerializer.Deserialize<RecaptchaResponse>(json);

        if (result == null || !result.Success)
        {
            Console.WriteLine($"[Auth] Recaptcha validation failed. Result was null: {result == null}. Success: {result?.Success}. Errors: {string.Join(", ", result?.ErrorCodes ?? Array.Empty<string>())}");
            return false;
        }

        // If it's a V3 response, it includes a score. Must be >= threshold.
        // If it's a V2 response, no score is provided, so we treat it as successful.
        if (result.Score.HasValue)
        {
            bool passed = result.Score.Value >= threshold;
            if (!passed) Console.WriteLine($"[Auth] Recaptcha validation failed: Score {result.Score.Value} is lower than threshold {threshold}");
            return passed;
        }

        Console.WriteLine("[Auth] Recaptcha validation succeeded (V2/Checkbox).");
        return true;
    }

    private class RecaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("score")]
        public double? Score { get; set; }
        [JsonPropertyName("action")]
        public string? Action { get; set; }
        [JsonPropertyName("challenge_ts")]
        public string? ChallengeTs { get; set; }
        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }
        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
