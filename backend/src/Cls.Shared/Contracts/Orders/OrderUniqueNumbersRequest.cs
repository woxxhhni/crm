namespace Cls.Shared.Contracts.Orders;

public class OrderUniqueNumbersRequest
{
    public IList<OrderUniqueNumberItem> Items { get; set; } = new List<OrderUniqueNumberItem>();
    public int? CreatedByUserId { get; set; }
}
