using Cls.Domain.Common;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class Stage : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public int OrderPosition { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ICollection<Step> Steps { get; set; } = new List<Step>();

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
