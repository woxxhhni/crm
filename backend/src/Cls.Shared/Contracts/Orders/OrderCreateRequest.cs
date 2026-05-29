namespace Cls.Shared.Contracts.Orders;

public class OrderCreateRequest
{
    public required string Title { get; set; }
    public required DateTime OrderDate { get; set; }
    public string? Description { get; set; }

    public required string BuyCurrency { get; set; }
    public required decimal BuyAmount { get; set; }
    public required string SellCurrency { get; set; }
    public required decimal SellAmount { get; set; }

    public int? ClientId { get; set; }
    public int? ProviderId { get; set; }
}
