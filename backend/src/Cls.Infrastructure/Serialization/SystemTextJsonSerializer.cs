using System.Text.Json;
using Cls.Application.Abstractions;

namespace Cls.Infrastructure.Serialization;

public sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, Options);

    public T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, Options);
}
