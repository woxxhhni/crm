namespace Cls.Shared.Contracts.Orders;

public class UpdatePaymentRequest
{
    public decimal Amount { get; set; }
    public string PaymentType { get; set; }
    public string? Description { get; set; }
    public List<int>? RemovedFileIds { get; set; }
}