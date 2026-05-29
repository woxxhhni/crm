namespace Cls.Shared.Contracts.Orders;

public class SetOrderStepRequest
{
    public int OrderId { get; set; }
    public int StepId { get; set; }
    public DateTime ActionDate { get; set; }
    public string? Description { get; set; }
}
