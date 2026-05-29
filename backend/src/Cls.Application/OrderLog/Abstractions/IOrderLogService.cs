using Cls.Domain.Entities;

namespace Cls.Application.OrderLogs.Abstractions;

public interface IOrderLogService
{
    INote Note { get; }
    IStatus Status { get; }
    IState Order { get; }
    IEmployee Employee { get; }
    IClientPayment ClientPayment { get; }
    IProviderPayment ProviderPayment { get; }
    IUniqueNumber UniqueNumber { get; }

    public interface INote
    {
        Task<int> Added(Note note, CancellationToken ct = default);
        Task<int> Edited(Note note, CancellationToken ct = default);
        Task<int> Removed(Note note, CancellationToken ct = default);
    }

    public interface IStatus
    {
        Task<int> Forward(Order order, Step fromStep, Step toStep, DateTime actionDate, string? description, List<int> fileIds, CancellationToken ct = default);
        Task<int> Backward(Order order, Step fromStep, Step toStep, DateTime actionDate, string? description, List<int> fileIds, CancellationToken ct = default);
        Task<int> Complete(Order order, Step step, DateTime actionDate, string? description, List<int> fileIds, CancellationToken ct = default);
    }

    public interface IState
    {
        Task<int> Completed(Order order, DateTime actionDate, string? description, CancellationToken ct = default);
        Task<int> Canceled(Order order, DateTime actionDate, string? description, CancellationToken ct = default);
        Task<int> Suspended(Order order, DateTime actionDate, string? description, CancellationToken ct = default);
        Task<int> ReturnedToProgress(Order order, DateTime actionDate, string? description, CancellationToken ct = default);
        Task<int> Created(Order order, CancellationToken ct = default);
        Task<int> Edited(Order order, CancellationToken ct = default);
    }

    public interface IEmployee
    {
        Task<int> Assigned(OrderEmployee employee, CancellationToken ct = default);
        Task<int> Removed(OrderEmployee employee, CancellationToken ct = default);
    }

    public interface IClientPayment
    {
        Task<int> Added(ClientOrderPayment payment, CancellationToken ct = default);
        Task<int> Edited(ClientOrderPayment payment, CancellationToken ct = default);
        Task<int> Removed(ClientOrderPayment payment, CancellationToken ct = default);
    }

    public interface IProviderPayment
    {
        Task<int> Added(ProviderOrderPayment payment, CancellationToken ct = default);
        Task<int> Edited(ProviderOrderPayment payment, CancellationToken ct = default);
        Task<int> Removed(ProviderOrderPayment payment, CancellationToken ct = default);
    }

    public interface IUniqueNumber
    {
        Task<int> Added(OrderUniqueNumber uniqueNumber, CancellationToken ct = default);
        Task<int> Removed(OrderUniqueNumber uniqueNumber, CancellationToken ct = default);
    }
}
