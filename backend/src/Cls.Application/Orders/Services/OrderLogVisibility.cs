using Cls.Domain.Enums;

namespace Cls.Application.Orders.Services;

public static class OrderLogVisibility
{
    private static readonly HashSet<OrderLogType> VisibleTypes =
    [
        OrderLogType.NoteAdded,
        OrderLogType.OrderCreated,
        OrderLogType.OrderSuspended,
        OrderLogType.OrderCompleted,
        OrderLogType.OrderCanceled,
        OrderLogType.OrderReturnedToProgress,
        OrderLogType.EmployeeAssigned,
        OrderLogType.StatusForward,
        OrderLogType.StatusBackward,
        OrderLogType.StatusComplete
    ];

    public static bool IsVisible(OrderLogType logType) => VisibleTypes.Contains(logType);
}
