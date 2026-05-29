using System.Text.Json.Serialization;
using Cls.Domain.Common;
namespace Cls.Domain.Entities;
public class ClientFileGroup : SoftDeletableEntity
{
    public int ClientId { get; set; }
    public required string Label { get; set; }

    // Navigation
    [JsonIgnore]
    public Client? Client { get; set; }
    public ICollection<ClientFileGroupItem> Items { get; set; } = new List<ClientFileGroupItem>();

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
