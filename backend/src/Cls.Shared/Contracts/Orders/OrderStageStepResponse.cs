namespace Cls.Shared.Contracts.Orders;

//public class OrderStageLogsResponse
//{
//    public int StageId { get; set; }
//    public string StageName { get; set; } = string.Empty;
//    public IList<OrderStepLogsResponse> Steps { get; set; } = new List<OrderStepLogsResponse>();
//}

public class OrderStepLogsResponse
{
    public int StepId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StepOrderPosition { get; set; }
    public DateTime? StepEnteredAt { get; set; }
    public DateTime? StepExitedAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public IList<OrderLogResponse> Logs { get; set; } = new List<OrderLogResponse>();
}
