using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class StageConfiguration : SoftDeletableEntityConfiguration<Stage>
{
    public override void Configure(EntityTypeBuilder<Stage> e)
    {
        base.Configure(e);

        e.ToTable("stages");
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        e.Property(x => x.OrderPosition).HasColumnName("order_position").IsRequired();
        e.HasIndex(x => x.OrderPosition).IsUnique().HasDatabaseName("idx_stages_order_position");
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
        e.HasIndex(x => x.IsActive).HasDatabaseName("idx_stages_is_active");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_stages_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_stages_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_stages_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_stages_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_stages_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_stages_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
