namespace Cls.Shared.Contracts.Orders;

public class ExtraProviderResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
