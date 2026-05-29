using Cls.Domain.Common;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class ExtraProvider : SoftDeletableEntity
{
    public int OrderId { get; set; }
    public int ProviderId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;
    public Provider Provider { get; set; } = null!;

    public ICollection<ExtraProviderOrderPayment> Payments { get; set; } = new List<ExtraProviderOrderPayment>();

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }

    public void Remove(int userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedByUserId = userId;
    }
}
