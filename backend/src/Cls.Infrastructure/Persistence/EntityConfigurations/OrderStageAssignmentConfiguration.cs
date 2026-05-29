using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderStageAssignmentConfiguration : SoftDeletableEntityConfiguration<OrderStageAssignment>
{
    public override void Configure(EntityTypeBuilder<OrderStageAssignment> e)
    {
        base.Configure(e);

        e.ToTable("order_stage_assignments");
        e.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        e.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
        e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();

        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_order_stage_assignments_order_id");
        e.HasIndex(x => x.StageId).HasDatabaseName("idx_order_stage_assignments_stage_id");
        e.HasIndex(x => x.UserId).HasDatabaseName("idx_order_stage_assignments_user_id");
        e.HasIndex(x => new { x.OrderId, x.StageId })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("idx_order_stage_assignments_order_stage_active");

        e.HasOne(x => x.Order).WithMany(o => o.StageAssignments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Stage).WithMany().HasForeignKey(x => x.StageId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
