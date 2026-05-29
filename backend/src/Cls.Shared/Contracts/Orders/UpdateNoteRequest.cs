namespace Cls.Shared.Contracts.Orders;

public class UpdateNoteRequest
{
    public DateTime ActionDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<int>? RemovedFileIds { get; set; }
}