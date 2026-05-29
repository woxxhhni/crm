using Cls.Domain.Common;
using Cls.Domain.Enums;
namespace Cls.Domain.Entities;
public class StoredFile : SoftDeletableEntity
{
    public required string OriginalFilename { get; set; }
    public required string StoredFilename { get; set; }
    public required string FilePath { get; set; }
    public required int FileSize { get; set; }
    public required string MimeType { get; set; }
    public UploadFileBucket Category { get; set; } = UploadFileBucket.GeneralBucket;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int UploadedByUserId { get; set; }

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}

