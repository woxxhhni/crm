using System.Text.Json.Serialization;
using Cls.Domain.Common;

namespace Cls.Domain.Entities;

public class OrderSellInvoice : SoftDeletableEntity
{
    public int OrderId { get; set; }
    public int FileId { get; set; }
    public DateTime UploadedAt { get; set; }
    [JsonIgnore]
    public Order Order { get; set; } = null!;
    [JsonIgnore]
    public StoredFile File { get; set; } = null!;

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
