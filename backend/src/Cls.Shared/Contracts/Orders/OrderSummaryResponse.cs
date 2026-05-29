namespace Cls.Shared.Contracts.Orders;

public class OrderSummaryResponse
{
    public int Total => InProgress + Canceled + Completed + Suspended;
    public int InProgress { get; set; }
    public int Canceled { get; set; }
    public int Completed { get; set; }
    public int Suspended { get; set; }
    public List<OrderStageSummaryResponse> StageSummaries { get; set; } = new();

    public class OrderStageSummaryResponse
    {
        public string Name { get; set; }
        public decimal Percent { get; set; }
    }
}