using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;

namespace Cls.Application.Users;

public static class UserRoleGuards
{
    public static void RejectAdminCreation(UserRole role)
    {
        if (role == UserRole.Admin)
            throw new BusinessException("You can not create admin user");
    }

    public static UserRole ParseRole(string? roleValue, UserRole fallback = UserRole.Employee)
    {
        return Enum.TryParse(roleValue, true, out UserRole role) ? role : fallback;
    }
}
