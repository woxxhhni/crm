namespace Cls.Shared.Contracts.Orders;

public class SetOrderStepCompleteRequest
{
    public int OrderId { get; set; }
    public DateTime ActionDate { get; set; }
    public string? Description { get; set; }
}
