using System.ComponentModel;

namespace Cls.Domain.Enums;

public enum OrderLogType
{
    // 1xxx – Note
    [Description("Note Added")]
    NoteAdded = 1001,
    [Description("Note Edited")]
    NoteEdited = 1002,
    [Description("Note Removed")]
    NoteRemoved = 1003,

    // 2xxx – StatusChange
    [Description("Status Forwarded")]
    StatusForward = 2001,
    [Description("Status Backward")]
    StatusBackward = 2002,
    [Description("Status Completed")]
    StatusComplete = 2003, 

    // 3xxx – OrderState
    [Description("Order Created")] 
    OrderCreated = 3001,
    [Description("Order Edited")]
    OrderEdited = 3002,
    [Description("Order Suspended")]
    OrderSuspended = 3003,
    [Description("Order Returned To Progress")]
    OrderReturnedToProgress = 3004,
    [Description("Order Completed")]
    OrderCompleted = 3005,
    [Description("Order Canceled")]
    OrderCanceled = 3006,

    // 4xxx – Employee
    [Description("Employee Assigned")]
    EmployeeAssigned = 4001,
    [Description("Employee Removed")]
    EmployeeRemoved = 4002,

    // 5xxx – ClientPayment
    [Description("Client Payment Added")]
    ClientPaymentAdded = 5001,
    [Description("Client Payment Edited")]
    ClientPaymentEdited = 5002,
    [Description("Client Payment Removed")]
    ClientPaymentRemoved = 5003,

    // 6xxx – ProviderPayment
    [Description("Provider Payment Added")]
    ProviderPaymentAdded = 6001,
    [Description("Provider Payment Edited")]
    ProviderPaymentEdited = 6002,
    [Description("Provider Payment Removed")]
    ProviderPaymentRemoved = 6003,

    // 7xxx – UniqueNumber
    [Description("Unique Number Added")]
    UniqueNumberAdded = 7001,
    [Description("Unique Number Removed")]
    UniqueNumberRemoved = 7002
}


public static class OrderLogTypeExtensions
{
    public static OrderLogCategory GetCategory(this OrderLogType type) => (OrderLogCategory)((int)type / 1000);

    public static bool IsNote(this OrderLogType type) => type.GetCategory() == OrderLogCategory.Note;
    public static bool IsStatusChange(this OrderLogType type) => type.GetCategory() == OrderLogCategory.StatusChange;
    public static bool IsOrder(this OrderLogType type) => type.GetCategory() == OrderLogCategory.Order;
    public static bool IsEmployee(this OrderLogType type) => type.GetCategory() == OrderLogCategory.Employee;
    public static bool IsClientPayment(this OrderLogType type) => type.GetCategory() == OrderLogCategory.ClientPayment;
    public static bool IsProviderPayment(this OrderLogType type) => type.GetCategory() == OrderLogCategory.ProviderPayment;
    public static bool IsUniqueNumber(this OrderLogType type) => type.GetCategory() == OrderLogCategory.UniqueNumber;
}
