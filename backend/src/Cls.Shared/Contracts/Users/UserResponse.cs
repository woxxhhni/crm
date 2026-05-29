namespace Cls.Shared.Contracts.Users;
public class UserResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public string UserFullName { get; set; }
    public int? UserProfileFileId { get; set; }
    public string UserProfileUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
