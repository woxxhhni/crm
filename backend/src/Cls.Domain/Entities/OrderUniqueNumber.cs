using System.Text.Json.Serialization;
using Cls.Domain.Common;

namespace Cls.Domain.Entities;

public class OrderUniqueNumber : SoftDeletableEntity
{
    public int OrderId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    [JsonIgnore]
    public Order Order { get; set; } = null!;
    
    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
