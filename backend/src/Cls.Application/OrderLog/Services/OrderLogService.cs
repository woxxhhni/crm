
using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Shared.Contracts.Abstractions;

namespace Cls.Application.OrderLogs.Services;

public sealed class OrderLogService : IOrderLogService
{
    public IOrderLogService.INote Note { get; }
    public IOrderLogService.IStatus Status { get; }
    public IOrderLogService.IState Order { get; }
    public IOrderLogService.IEmployee Employee { get; }
    public IOrderLogService.IClientPayment ClientPayment { get; }
    public IOrderLogService.IProviderPayment ProviderPayment { get; }
    public IOrderLogService.IUniqueNumber UniqueNumber { get; }

    public OrderLogService(IUnitOfWork uow, ICurrentUserService currentUser, IJsonSerializer json)
    {
        Note = new NoteOrderLogService(uow, currentUser, json);
        Status = new StatusOrderLogService(uow, currentUser, json);
        Order = new StateOrderLogService(uow, currentUser, json);
        Employee = new EmployeeOrderLogService(uow, currentUser, json);
        ClientPayment = new ClientPaymentOrderLogService(uow, currentUser, json);
        ProviderPayment = new ProviderPaymentOrderLogService(uow, currentUser, json);
        UniqueNumber = new UniqueNumberOrderLogService(uow, currentUser, json);
    }
}
