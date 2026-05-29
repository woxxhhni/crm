using System.Text.Json.Serialization;
using Cls.Domain.Common;
using Cls.Domain.Enums;
using Cls.Shared.Exceptions;

namespace Cls.Domain.Entities;

public class OrderPayment : SoftDeletableEntity
{
    public int OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public OrderPaymentType PaymentType { get; private set; }
    public string? Description { get; private set; }
    public DateOnly PaymentDate { get; private set; }

    [JsonIgnore]
    public Order Order { get; private set; } = null!;
    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
    protected OrderPayment(){}
    protected OrderPayment(int orderId, decimal amount, OrderPaymentType paymentType, string? description, int userId)
    {
        GuardAgainstInvalidAmount(amount);

        OrderId = orderId;
        Amount = amount;
        PaymentType = paymentType;
        Description = description;
        PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        CreatedByUserId = userId;
        UpdatedByUserId = userId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(decimal amount, OrderPaymentType paymentType, string? description, int userId)
    {
        GuardAgainstInvalidAmount(amount);

        Amount = amount;
        PaymentType = paymentType;
        Description = description;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Remove(int userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedByUserId = userId;
    }

    private static void GuardAgainstInvalidAmount(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidActionException("Payment amount must be greater than zero.");
    }
}