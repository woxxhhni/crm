using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class NoteConfiguration : SoftDeletableEntityConfiguration<Note>
{
    public override void Configure(EntityTypeBuilder<Note> e)
    {
        base.Configure(e);

        e.ToTable("notes");
        e.Property(x => x.Title).HasColumnName("title").IsRequired();
        e.Property(x => x.Description).HasColumnName("description").IsRequired();
        e.Property(x => x.NoteDate).HasColumnName("note_date").IsRequired();
        e.HasIndex(x => x.NoteDate).HasDatabaseName("idx_notes_note_date");
        e.Property(x => x.ActionDate).HasColumnName("action_date");
        e.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_notes_order_id");
        e.HasOne(x => x.Order).WithMany(o => o.Notes).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.StepId).HasColumnName("step_id").IsRequired();
        e.HasIndex(x => x.StepId).HasDatabaseName("idx_notes_step_id");
        e.HasOne(x => x.Step).WithMany(s => s.Notes).HasForeignKey(x => x.StepId).OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);

      

    }
}
