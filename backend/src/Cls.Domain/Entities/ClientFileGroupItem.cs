using System.Text.Json.Serialization;
using Cls.Domain.Common;
namespace Cls.Domain.Entities;
public class ClientFileGroupItem : SoftDeletableEntity
{
    public int ClientFileGroupId { get; set; }
    public int FileId { get; set; }
    [JsonIgnore]
    public ClientFileGroup? Group { get; set; }
    [JsonIgnore]
    public StoredFile? File { get; set; }

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
