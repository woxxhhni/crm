using Cls.Domain.Entities;

namespace Cls.Application.Orders.Services;

internal static class OrderAccess
{
    internal static bool IsAssignedToUser(Order order, int userId) =>
        order.Employees.Any(x => !x.IsDeleted && x.UserId == userId) ||
        order.StageAssignments.Any(x => !x.IsDeleted && x.UserId == userId);
}
