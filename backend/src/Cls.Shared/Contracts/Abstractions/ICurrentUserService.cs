using Cls.Shared.Contracts.Users;

namespace Cls.Shared.Contracts.Abstractions;

public interface ICurrentUserService
{
    int UserId { get; set; }
    UserRole Role { get; set; }
}
