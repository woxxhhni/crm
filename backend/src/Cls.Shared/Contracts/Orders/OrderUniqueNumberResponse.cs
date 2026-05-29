namespace Cls.Shared.Contracts.Orders;

public class OrderUniqueNumberResponse
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
