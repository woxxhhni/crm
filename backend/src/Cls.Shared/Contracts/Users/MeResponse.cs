using System.Text.Json.Serialization;

namespace Cls.Shared.Contracts.Users;

public class MeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    [JsonIgnore]
    public int? ProfileFileId { get; set; }
    public string ProfileUrl { get; set; } = string.Empty;
}
