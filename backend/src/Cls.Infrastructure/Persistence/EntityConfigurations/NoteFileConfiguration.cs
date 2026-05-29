using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class NoteFileConfiguration : SoftDeletableEntityConfiguration<NoteFile>
{
    public override void Configure(EntityTypeBuilder<NoteFile> e)
    {
        base.Configure(e);
        e.ToTable("note_files");
        e.Property(x => x.NoteId).IsRequired().HasColumnName("note_id");
        e.Property(x => x.FileId).IsRequired().HasColumnName("file_id");
        e.HasIndex(x => x.NoteId).HasDatabaseName("idx_note_files_note_id");
        e.HasIndex(x => x.FileId).HasDatabaseName("idx_note_files_file_id");
        e.HasOne(x => x.Note).WithMany(n => n.Files).HasForeignKey(x => x.NoteId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
      
    }
}