using Cls.Domain.Common;

namespace Cls.Domain.Entities;

public class OrderSequence : SoftDeletableEntity
{
    public int Year { get; set; }
    public int LastNumber { get; set; }
    public string Prefix { get; set; } = "ORD";

    public User CreatedByUser { get; set; } = default!;
    public User UpdatedByUser { get; set; } = default!;
    public User? DeletedByUser { get; set; }
}
