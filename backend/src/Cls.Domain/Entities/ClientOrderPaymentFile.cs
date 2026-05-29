using Cls.Domain.Common;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class ClientOrderPaymentFile: SoftDeletableEntity
{
    public int PaymentId { get; private set; }
    public int FileId { get; private set; }

    [JsonIgnore]
    public ClientOrderPayment Payment { get; private set; } = null!;
    [JsonIgnore]
    public StoredFile File { get; private set; } = null!;
    [JsonIgnore]
    public User CreatedByUser { get; set; } = default!;
    [JsonIgnore]
    public User UpdatedByUser { get; set; } = default!;
    [JsonIgnore]
    public User? DeletedByUser { get; set; }
    private ClientOrderPaymentFile() { }
    public ClientOrderPaymentFile(int fileId, int userId)
    {
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