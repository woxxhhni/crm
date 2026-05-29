using Cls.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        e.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id").IsRequired();
        e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
    }
}

public abstract class SoftDeletableEntityConfiguration<T> : BaseEntityConfiguration<T> where T : SoftDeletableEntity
{
    public override void Configure(EntityTypeBuilder<T> e)
    {
        base.Configure(e);
        e.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        e.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");
        e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}