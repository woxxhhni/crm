
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class StepConfiguration : SoftDeletableEntityConfiguration<Step>
{
    public override void Configure(EntityTypeBuilder<Step> e)
    {
        base.Configure(e);

        e.ToTable("steps");
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        e.Property(x => x.OrderPosition).HasColumnName("order_position").IsRequired();
        e.HasIndex(x => x.OrderPosition).HasDatabaseName("idx_steps_order_position");
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.IsFinalStep).HasColumnName("is_final_step").IsRequired().HasDefaultValue(false);
        e.HasIndex(x => x.IsFinalStep).HasDatabaseName("idx_steps_is_final");
        e.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
        e.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
        e.HasIndex(x => x.StageId).HasDatabaseName("idx_steps_stage_id");
        e.HasOne(x => x.Stage).WithMany(s => s.Steps).HasForeignKey(x => x.StageId).OnDelete(DeleteBehavior.Cascade); 
        e.HasIndex(x => new { x.StageId, x.OrderPosition }).IsUnique().HasDatabaseName("idx_steps_stage_order");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_steps_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_steps_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_steps_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_steps_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_steps_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_steps_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
