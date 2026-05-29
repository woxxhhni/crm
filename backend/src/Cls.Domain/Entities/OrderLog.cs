using Cls.Domain.Common;
using Cls.Domain.Enums;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class OrderLog : BaseEntity
{
    public int OrderId { get; set; }
    public int? StepId { get; set; }
    public int? NoteId { get; set; }

    public OrderLogType LogType { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public int? FromStepId { get; set; }
    public int? ToStepId { get; set; }

    public string? Metadata { get; set; }  // jsonb in db; store as string

    public int ActorUserId { get; set; }
    public DateTime LogDate { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;
    [JsonIgnore]
    public Step? Step { get; set; }
    [JsonIgnore]
    public Note? Note { get; set; }

    [JsonIgnore]
    public Step? FromStep { get; set; }
    [JsonIgnore]
    public Step? ToStep { get; set; }

    public User ActorUser { get; set; } = null!;

    [JsonIgnore]
    public List<OrderLogFile> Files { get; set; } = default!;
}