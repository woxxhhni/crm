using Cls.Domain.Common;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class OrderStageAssignment : SoftDeletableEntity
{
    public int OrderId { get; set; }
    public int StageId { get; set; }
    public int UserId { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;
    public Stage Stage { get; set; } = null!;
    public User User { get; set; } = null!;
    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
