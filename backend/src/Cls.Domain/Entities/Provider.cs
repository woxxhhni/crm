using Cls.Domain.Common;
namespace Cls.Domain.Entities;
public partial class Provider : SoftDeletableEntity
{
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public string? SecondPhone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? ProfileFileId { get; set; }
    public void SetProfileFile(int? fileId) => ProfileFileId = fileId;

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
    public ICollection<ProviderFileGroup> FileGroups { get; set; }
}
