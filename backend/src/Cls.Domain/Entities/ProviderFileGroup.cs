using System.Text.Json.Serialization;
using Cls.Domain.Common;
namespace Cls.Domain.Entities;
public class ProviderFileGroup : SoftDeletableEntity
{
    public int ProviderId { get; set; }
    public required string Label { get; set; }
    [JsonIgnore]
    public Provider? Provider { get; set; }
    public ICollection<ProviderFileGroupItem> Items { get; set; } = new List<ProviderFileGroupItem>();

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
