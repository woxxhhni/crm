using System.Text.Json.Serialization;
using Cls.Domain.Common;

namespace Cls.Domain.Entities;

public class OrderEmployee : SoftDeletableEntity
{
    public int OrderId { get; set; }
    public int UserId { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;
    public User User { get; set; } = null!;
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
