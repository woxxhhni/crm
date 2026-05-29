using Cls.Domain.Common;
using System.Text.Json.Serialization;

namespace Cls.Domain.Entities;

public class Note : SoftDeletableEntity
{
    public int OrderId { get; private set; }
    public int StepId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime NoteDate { get; private set; }
    public DateTime? ActionDate { get; private set; }

    [JsonIgnore]
    public Order Order { get; private set; } = null!;
    [JsonIgnore]
    public Step Step { get; private set; } = null!;
    //TODO: move to base class
    public User CreatedByUser { get; private set; } = null!;
    public User UpdatedByUser { get; private set; } = null!;
    public User? DeletedByUser { get; private set; }
    [JsonIgnore]
    public ICollection<NoteFile> Files { get; set; } = new List<NoteFile>();

    private Note() { }
    public Note(int orderId, int stepId, DateTime noteDate, string title, string description, List<int> fileIds, int userId)
    {
        OrderId = orderId;
        StepId = stepId;
        NoteDate = noteDate;
        ActionDate = noteDate;
        Title = title;
        Description = description;
        AddFiles(fileIds, userId);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedByUserId = userId;
        UpdatedByUserId = userId;
    }

    public void Update(DateTime date, string title, string description, List<int> fileIds, List<int> removedFileIds, int userId)
    {
        NoteDate = date;
        ActionDate = date;
        Title = title;
        Description = description;
        AddFiles(fileIds, userId);
        RemoveFiles(removedFileIds, userId);
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }

    private void AddFiles(List<int> fileIds, int userId)
    {
        fileIds.ForEach(x => Files.Add(new NoteFile(x, userId)));
    }

    private void RemoveFiles(List<int> fileIds, int userId)
    {
        foreach (var id in fileIds)
        {
            var file = Files.FirstOrDefault(x => x.FileId == id);
            if (file == null) continue;
            file.Remove(userId);
        }
    }
}
