namespace Cls.Shared.Contracts.Orders;

public class OrderResponse
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string? Description { get; set; }

    public string BuyCurrency { get; set; } = string.Empty;
    public decimal BuyAmount { get; set; }
    public string SellCurrency { get; set; } = string.Empty;
    public decimal SellAmount { get; set; }

    public decimal? ClientBalance { get; set; }
    public decimal? ProviderBalance { get; set; }
    public DateTime? BalancesLastCalculatedAt { get; set; }

    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
    public int CurrentStepId { get; set; }
    public string CurrentStepName { get; set; } = string.Empty;
    public int CurrentStageId { get; set; }
    public string CurrentStageName { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }

    public string Status { get; set; } = "in_progress";
    public DateTime? FirstActionDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public DateTime? SuspendedAt { get; set; }

    public IList<OrderInvoiceFileResponse> SellInvoiceLinks { get; set; } = new List<OrderInvoiceFileResponse>();
    public IList<OrderInvoiceFileResponse> BuyInvoiceLinks { get; set; } = new List<OrderInvoiceFileResponse>();
    public IList<OrderEmployeeResponse> Employees { get; set; } = new List<OrderEmployeeResponse>();
    public IList<OrderStageAssigneeResponse> StageAssignments { get; set; } = new List<OrderStageAssigneeResponse>();
    public IList<OrderUniqueNumberResponse> UniqueNumbers { get; set; } = new List<OrderUniqueNumberResponse>();
    //public IList<OrderLogResponse> Logs { get; set; } = new List<OrderLogResponse>();
    //public IList<OrderStageLogsResponse> Stages { get; set; } = new List<OrderStageLogsResponse>();
    public IList<OrderStepLogsResponse> Steps { get; set; } = new List<OrderStepLogsResponse>();
    public IList<ExtraProviderResponse> ExtraProviders { get; set; } = new List<ExtraProviderResponse>();
}
