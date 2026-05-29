namespace Cls.Shared.Contracts.Orders;

public class CreateExtraProviderRequest
{
    public int ProviderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
