
using Cls.Domain.Common;

namespace Cls.Domain.Entities;

public class OrderLogFile : BaseEntity
{
    public int OrderLogId { get; private set; }
    public int FileId { get; private set; }

    public OrderLog OrderLog { get; private set; } = null!;
    public StoredFile File { get; private set; } = null!;
    public User CreatedByUser { get; set; } = default!;
 

    private OrderLogFile() { }
    public OrderLogFile(int fileId, int userId)
    {
        FileId = fileId;
        CreatedByUserId = userId;
    }
}