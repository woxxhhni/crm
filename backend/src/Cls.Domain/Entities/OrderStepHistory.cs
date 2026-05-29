using Cls.Domain.Common;
using Cls.Domain.Enums;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class OrderStepHistory : SoftDeletableEntity
{
    public int OrderId { get; set; }
    public int StepId { get; set; }

    public DateTime EnteredAt { get; set; }
    public DateTime? ExitedAt { get; set; }

    public OrderStepEntryType EntryType { get; set; }
    public OrderStepExitReason? ExitReason { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;
    [JsonIgnore]
    public Step Step { get; set; } = null!;

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
