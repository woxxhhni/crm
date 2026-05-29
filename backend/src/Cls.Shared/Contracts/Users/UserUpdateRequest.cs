using System.ComponentModel.DataAnnotations;

namespace Cls.Shared.Contracts.Users;
public class UserUpdateRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    [Phone]
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; } = true;
}
