using System.Text.Json.Serialization;
using Cls.Domain.Common;
using Cls.Shared.Contracts.Users;
namespace Cls.Domain.Entities;
public class User : SoftDeletableEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int? FileId { get; set; }
    [JsonIgnore]
    public StoredFile? File { get; set; }

}
