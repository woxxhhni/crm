namespace Cls.Shared.Contracts.Orders;

public class OrderEmployeeResponse
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AssignedAt { get; set; }
    public int AssignedByUserId { get; set; }

    public DateTime? RemovedAt { get; set; }
    public int? RemovedByUserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int? ProfileFileId { get; set; }
    public string? ProfileUrl { get; set; }

    // Uncomment if needed
    //public string AssignedByName { get; set; } = string.Empty;
    //public string AssignedByEmail { get; set; } = string.Empty;
    //public string RemovedByName { get; set; } = string.Empty;
    //public string RemovedByEmail { get; set; } = string.Empty;
}
