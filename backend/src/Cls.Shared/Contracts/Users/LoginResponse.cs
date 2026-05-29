using System.Text.Json.Serialization;

namespace Cls.Shared.Contracts.Users;

public class LoginResponse
{
    [JsonPropertyName("token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires")]
    public int ExpireIn { get; set; }

    [JsonPropertyName("type")]
    public string? TokenType { get; set; }
}