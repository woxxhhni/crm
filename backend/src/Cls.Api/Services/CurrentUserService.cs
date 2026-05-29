using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Users;

public class CurrentUserService : ICurrentUserService
{
    public int UserId { get; set; }
    public UserRole Role { get; set; }
}
