namespace Cls.Shared.Contracts.Orders;

public class CreatePaymentRequest
{
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string? Description { get; set; }
}
