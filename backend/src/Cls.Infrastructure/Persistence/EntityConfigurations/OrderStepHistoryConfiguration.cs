using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class OrderStepHistoryConfiguration : SoftDeletableEntityConfiguration<OrderStepHistory>
{
    public override void Configure(EntityTypeBuilder<OrderStepHistory> e)
    {
        base.Configure(e);

        e.ToTable("order_step_historys");
        e.Property(x => x.EnteredAt).HasColumnName("entered_at").IsRequired();
        e.Property(x => x.EntryType).HasColumnName("entry_type").IsRequired();
        e.Property(x => x.ExitedAt).HasColumnName("exited_at");
        e.Property(x => x.ExitReason).HasColumnName("exit_reason");
        e.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_order_step_history_order_id");
        e.HasOne(x => x.Order).WithMany(o => o.StepHistory).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.StepId).HasColumnName("step_id").IsRequired();
        e.HasIndex(x => x.StepId).HasDatabaseName("idx_order_step_history_step_id");
        e.HasOne(x => x.Step).WithMany(s => s.StepHistory).HasForeignKey(x => x.StepId).OnDelete(DeleteBehavior.Restrict);
        e.HasIndex(x => new { x.OrderId, x.EnteredAt }).HasDatabaseName("idx_order_step_history_order_entered");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_order_step_historys_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_order_step_historys_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_order_step_historys_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_order_step_historys_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_order_step_historys_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_order_step_historys_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
