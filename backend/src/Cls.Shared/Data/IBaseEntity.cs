namespace Cls.Shared.Data;

public interface IBaseEntity
{
    int Id { get; set; }
    DateTime CreatedAt { get; set; }
    int CreatedByUserId { get; set; }
    DateTime UpdatedAt { get; set; }
    int UpdatedByUserId { get; set; }
}