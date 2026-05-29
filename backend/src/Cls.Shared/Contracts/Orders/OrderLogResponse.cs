namespace Cls.Shared.Contracts.Orders;

public class OrderLogResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? StepId { get; set; }
    public string? StepName { get; set; }
    public int? StageId { get; set; }
    public string? StageName { get; set; }
    public int? NoteId { get; set; }
    public string? NoteTitle { get; set; }
    public string LogType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? FromStepId { get; set; }
    public string? FromStepName { get; set; }
    public int? ToStepId { get; set; }
    public string? ToStepName { get; set; }
    public int ActorUserId { get; set; }
    public string ActorFullName { get; set; } = string.Empty;
    public int? ActorProfileFileId { get; set; }
    public string ActorProfileUrl { get; set; } = string.Empty;
    public DateTime? StepExitedAt { get; set; }
    public DateTime LogDate { get; set; }
    public List<FileResponse> Files { get; set; } = default!;
}