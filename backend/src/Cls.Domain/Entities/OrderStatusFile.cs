using Cls.Domain.Common;
using Cls.Domain.Enums;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class OrderStatusFile : SoftDeletableEntity
{
    public int OrderId { get; private set; }
    public int FileId { get; private set; }
    public OrderStatus OrderStatus { get; private set; }

    [JsonIgnore]
    public Order Order { get; private set; } = null!;
    [JsonIgnore]
    public StoredFile File { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = default!;
    public User UpdatedByUser { get; private set; } = default!;
    public User? DeletedByUser { get; private set; }

    private OrderStatusFile() { }
    public OrderStatusFile(int orderId, int fileId, OrderStatus status, int userId)
    {
        OrderId = orderId;
        OrderStatus = status;
        FileId = fileId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedByUserId = userId;
        UpdatedByUserId = userId;
    }
    public void Remove(int userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedByUserId = userId;
    }
}