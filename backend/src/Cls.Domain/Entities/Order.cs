using Cls.Domain.Common;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using Cls.Shared.Mapping;

namespace Cls.Domain.Entities;

public class Order : SoftDeletableEntity
{
    public required string OrderNumber { get; set; }
    public required string Title { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Description { get; set; }

    public required string BuyCurrency { get; set; }
    public decimal BuyAmount { get; set; }
    public required string SellCurrency { get; set; }
    public decimal SellAmount { get; set; }

    public decimal? ClientBalance { get; set; }
    public decimal? ProviderBalance { get; set; }
    public DateTime? BalancesLastCalculatedAt { get; set; }

    public int ClientId { get; set; }
    public int ProviderId { get; set; }
    public int CurrentStepId { get; set; }

    public OrderStatus Status { get; private set; } = OrderStatus.InProgress;
    public DateTime? FirstActionDate { get; set; }

    public DateTime? CompletedAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }
    public DateTime? SuspendedAt { get; private set; }

    public Client Client { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
    public Step CurrentStep { get; set; } = default!;

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }

    public ICollection<OrderSellInvoice> SellInvoices { get; set; } = new List<OrderSellInvoice>();
    public ICollection<OrderBuyInvoice> BuyInvoices { get; set; } = new List<OrderBuyInvoice>();
    public ICollection<OrderEmployee> Employees { get; set; } = new List<OrderEmployee>();
    public ICollection<OrderStageAssignment> StageAssignments { get; set; } = new List<OrderStageAssignment>();
    public ICollection<OrderUniqueNumber> UniqueNumbers { get; set; } = new List<OrderUniqueNumber>();
    public ICollection<OrderStepHistory> StepHistory { get; set; } = new List<OrderStepHistory>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<OrderLog> Logs { get; set; } = new List<OrderLog>();
    public ICollection<ClientOrderPayment> ClientPayments { get; private set; } = new List<ClientOrderPayment>();
    public ICollection<ProviderOrderPayment> ProviderPayments { get; private set; } = new List<ProviderOrderPayment>();
    public ICollection<ExtraProvider> ExtraProviders { get; private set; } = new List<ExtraProvider>();
    public ICollection<OrderStatusFile> StatusFiles { get; private set; } = new List<OrderStatusFile>();

    public void AddStepHistory(OrderStepHistory history)
    {
        StepHistory.Add(history);
    }

    public void AddClientPayment(ClientOrderPayment payment)
    {
        GuardAgainstInvalidStatus();
        ClientPayments.Add(payment);
        UpdateClientBalance(payment);
    }

    public void UpdateClientPayment(int id, decimal amount, OrderPaymentType paymentType, string? description
        , List<int> fileIds, List<int> removedFileIds, int userId)
    {
        GuardAgainstInvalidStatus();
        var payment = ClientPayments.First(x => x.Id == id);
        if (payment is null)
            throw new NotFoundException("Payment not found");

        payment.Update(amount, paymentType, description, fileIds, removedFileIds, userId);
        UpdateClientBalance(payment);
    }

    public void RemoveClientPayment(ClientOrderPayment payment, int userId)
    {
        GuardAgainstInvalidStatus();
        payment.Remove(userId);
        UpdateClientBalance(payment);
    }


    public void AddProviderPayment(ProviderOrderPayment payment)
    {
        GuardAgainstInvalidStatus();
        ProviderPayments.Add(payment);
        UpdateProviderBalance(payment);
    }

    public void UpdateProviderPayment(int id, decimal amount, OrderPaymentType paymentType, string? description
        , List<int> fileIds, List<int> removedFileIds, int userId)
    {
        GuardAgainstInvalidStatus();
        var payment = ProviderPayments.First(x => x.Id == id);
        if (payment is null)
            throw new NotFoundException("Payment not found");
        payment.Update(amount, paymentType, description, fileIds, removedFileIds, userId);
        UpdateProviderBalance(payment);
    }

    public void RemoveProviderPayment(ProviderOrderPayment payment, int userId)
    {
        GuardAgainstInvalidStatus();
        payment.Remove(userId);
        UpdateProviderBalance(payment);
    }

    public void AddNote(Note note, ICurrentUserService currentUserService)
    {
        GuardAgainstInvalidEmployee(currentUserService);
        GuardAgainstInvalidStatus();
        Notes.Add(note);
    }

    public void UpdateNote(int id, DateTime date, string title, string description, List<int> fileIds, List<int> removedFileIds, ICurrentUserService currentUserService)
    {
        var note = Notes.FirstOrDefault(x => x.Id == id);
        if (note is null)
            throw new NotFoundException("Note not found");

        GuardAgainstInvalidEmployee(currentUserService);
        GuardAgainstInvalidStatus();
        if (note.CreatedByUserId != currentUserService.UserId)
            throw new InvalidAccessException();

        note.Update(date, title, description, fileIds, removedFileIds, currentUserService.UserId);
    }

    public void Complete(DateTime actionDate, List<int> fileIds, List<int> removedFileIds, int userId)
    {
        GuardAgainstInvalidStatus();
        Status = OrderStatus.Completed;
        CompletedAt = actionDate;
        AddStatusFiles(fileIds, userId);
        RemoveStatusFiles(removedFileIds, userId);
    }

    public void Cancel(DateTime actionDate, List<int> fileIds, List<int> removedFileIds, ICurrentUserService currentUserService)
    {
        GuardAgainstInvalidEmployee(currentUserService);
        GuardAgainstInvalidStatus();
        Status = OrderStatus.Canceled;
        CanceledAt = actionDate;
        AddStatusFiles(fileIds, currentUserService.UserId);
        RemoveStatusFiles(removedFileIds, currentUserService.UserId);
    }

    public void Suspend(DateTime actionDate, List<int> fileIds, List<int> removedFileIds, int userId)
    {
        GuardAgainstInvalidStatus();
        Status = OrderStatus.Suspended;
        SuspendedAt = actionDate;
        AddStatusFiles(fileIds, userId);
        RemoveStatusFiles(removedFileIds, userId);
    }

    public void UnSuspended(List<int> fileIds, List<int> removedFileIds, int userId)
    {
        if (Status != OrderStatus.Suspended) throw new InvalidActionException();
        Status = OrderStatus.InProgress;
        CompletedAt = null;
        CanceledAt = null;
        SuspendedAt = null;
        AddStatusFiles(fileIds, userId);
        RemoveStatusFiles(removedFileIds, userId);
    }

    public void Reopen(List<int> fileIds, List<int> removedFileIds, int userId)
    {
        if (Status != OrderStatus.Completed) throw new InvalidActionException();
        Status = OrderStatus.InProgress;
        CompletedAt = null;
        CanceledAt = null;
        SuspendedAt = null;
        AddStatusFiles(fileIds, userId);
        RemoveStatusFiles(removedFileIds, userId);
    }

    private void UpdateClientBalance(ClientOrderPayment payment)
    {
        ClientBalance ??= 0;
        ClientBalance += payment.Amount;
        BalancesLastCalculatedAt = DateTime.UtcNow;
    }

    private void UpdateProviderBalance(ProviderOrderPayment payment)
    {
        ProviderBalance ??= 0;
        ProviderBalance += payment.Amount;
        BalancesLastCalculatedAt = DateTime.UtcNow;
    }

    private void GuardAgainstInvalidStatus()
    {
        if (Status != OrderStatus.InProgress)
            throw new InvalidActionException("Order is not in progress you can not change it");
    }

    private void GuardAgainstInvalidEmployee(ICurrentUserService currentUserService)
    {
        if (currentUserService.Role != UserRole.Employee) return;
        if (!Employees.Any(x => x.UserId == currentUserService.UserId) &&
            !StageAssignments.Any(x => x.UserId == currentUserService.UserId))
            throw new InvalidAccessException();
    }

    private void AddStatusFiles(List<int> fileIds, int userId)
    {
        fileIds.ForEach(x => StatusFiles.Add(new OrderStatusFile(Id, x, Status, userId)));
    }

    private void RemoveStatusFiles(List<int> fileIds, int userId)
    {
        foreach (var id in fileIds)
        {
            var file = StatusFiles.FirstOrDefault(x => x.FileId == id);
            if (file == null) continue;
            file.Remove(userId);
        }
    }

    public void UpdateEmployees(List<int>? employeeUserIds, int currentUserId)
    {
        var existingEmployeeUserIds = Employees.Select(x => x.UserId).ToList();
        var employeesToAdd = employeeUserIds?.Except(existingEmployeeUserIds).ToList() ?? new List<int>();
        var employeesToRemove = existingEmployeeUserIds.Except(employeeUserIds ?? Enumerable.Empty<int>()).ToList();
        foreach (var userId in employeesToAdd)
        {
            Employees.Add(new OrderEmployee
            {
                OrderId = Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserId,
                UpdatedByUserId = currentUserId
            });
        }

        foreach (var userId in employeesToRemove)
        {
            var employee = Employees.First(x => x.UserId == userId);
            employee.Remove(currentUserId);
        }
    }
}
