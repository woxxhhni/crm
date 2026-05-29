using Cls.Domain.Common;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class Step : SoftDeletableEntity
{
    public int StageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OrderPosition { get; set; }
    public string? Description { get; set; }
    public bool IsFinalStep { get; set; }
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public Stage Stage { get; set; } = null!;
    [JsonIgnore]
    public ICollection<OrderStepHistory> StepHistory { get; set; } = new List<OrderStepHistory>();
    [JsonIgnore]
    public ICollection<Note> Notes { get; set; } = new List<Note>();

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
