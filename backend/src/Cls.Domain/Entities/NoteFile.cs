using Cls.Domain.Common;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class NoteFile : SoftDeletableEntity
{
    public int NoteId { get; private set; }
    public int FileId { get; private set; }

    [JsonIgnore]
    public Note Note { get; private set; } = null!;
    [JsonIgnore]
    public StoredFile File { get; private set; } = null!;
    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }

    private NoteFile() { }
    public NoteFile(int fileId, int userId)
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
