namespace Cls.Shared.Contracts.Orders;

public class OrderStageAssigneeResponse
{
    public int StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StageOrderPosition { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeEmail { get; set; }
    public int? ProfileFileId { get; set; }
    public string? ProfileUrl { get; set; }
    public DateTime? AssignedAt { get; set; }
    public int? AssignedByUserId { get; set; }
}
