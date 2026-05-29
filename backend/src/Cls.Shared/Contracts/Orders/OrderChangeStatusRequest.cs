namespace Cls.Shared.Contracts.Orders;

public class OrderChangeStatusRequest
{
    public required DateTime ActionDate { get; set; }
    public string? Description { get; set; }
    public List<int>? RemovedFileIds { get; set; }
}
