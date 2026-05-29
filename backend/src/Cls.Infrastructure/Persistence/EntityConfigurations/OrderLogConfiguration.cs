using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderLogConfiguration : BaseEntityConfiguration<OrderLog>
{
    public override void Configure(EntityTypeBuilder<OrderLog> e)
    {
        base.Configure(e);

        e.ToTable("order_logs");

        e.Property(x => x.LogType).HasColumnName("log_type").IsRequired();
        e.Property(x => x.ActorUserId).HasColumnName("actor_user_id").IsRequired();
        e.HasIndex(x => x.ActorUserId).HasDatabaseName("idx_order_logs_actor_user_id");
        e.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        e.Property(x => x.LogDate).HasColumnName("log_date").IsRequired();
        e.Property(x => x.Title).HasColumnName("title").HasMaxLength(255);
        e.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_order_logs_order_id");
        e.HasOne(x => x.Order).WithMany(o => o.Logs).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.StepId).HasColumnName("step_id");
        e.HasIndex(x => x.StepId).HasDatabaseName("idx_order_logs_step_id");
        e.HasOne(x => x.Step).WithMany().HasForeignKey(x => x.StepId).OnDelete(DeleteBehavior.Restrict);
        e.Property(x => x.NoteId).HasColumnName("note_id");
        e.HasIndex(x => x.NoteId).HasDatabaseName("idx_order_logs_note_id");
        e.HasOne(x => x.Note).WithMany().HasForeignKey(x => x.NoteId).OnDelete(DeleteBehavior.Restrict);
        e.HasIndex(x => x.LogType).HasDatabaseName("idx_order_logs_log_type");
        e.HasIndex(x => x.LogDate).HasDatabaseName("idx_order_logs_log_date");
        e.HasIndex(x => new { x.OrderId, x.LogDate }).HasDatabaseName("idx_order_logs_order_log_date");
        e.HasIndex(x => new { x.LogType, x.LogDate }).HasDatabaseName("idx_order_logs_type_date");
        e.Property(x => x.FromStepId).HasColumnName("from_step_id");
        e.HasOne(x => x.FromStep).WithMany().HasForeignKey(x => x.FromStepId).OnDelete(DeleteBehavior.Restrict);
        e.Property(x => x.ToStepId).HasColumnName("to_step_id");
        e.HasOne(x => x.ToStep).WithMany().HasForeignKey(x => x.ToStepId).OnDelete(DeleteBehavior.Restrict);
        e.Property(x => x.Metadata).HasColumnName("metadata");
    }
}
