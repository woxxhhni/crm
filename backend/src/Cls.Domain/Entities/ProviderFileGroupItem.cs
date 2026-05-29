using System.Text.Json.Serialization;
using Cls.Domain.Common;
namespace Cls.Domain.Entities;
public class ProviderFileGroupItem : SoftDeletableEntity
{
    public int ProviderFileGroupId { get; set; }
    public int FileId { get; set; }

    [JsonIgnore]
    public ProviderFileGroup? Group { get; set; }
    [JsonIgnore]
    public StoredFile? File { get; set; }

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
