using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class StoredFileConfiguration : SoftDeletableEntityConfiguration<StoredFile>
{
    public override void Configure(EntityTypeBuilder<StoredFile> e)
    {
        base.Configure(e);

        e.ToTable("files");
        e.Property(x => x.OriginalFilename).HasColumnName("original_filename").HasMaxLength(255).IsRequired();
        e.Property(x => x.StoredFilename).HasColumnName("stored_filename").HasMaxLength(255).IsRequired();
        e.HasIndex(x => x.StoredFilename).IsUnique().HasDatabaseName("idx_files_stored_filename");
        e.Property(x => x.FilePath).HasColumnName("file_path").HasMaxLength(500).IsRequired();
        e.Property(x => x.FileSize).HasColumnName("file_size").IsRequired();
        e.Property(x => x.MimeType).HasColumnName("mime_type").HasMaxLength(100).IsRequired();
        e.Property(x => x.Category).HasColumnName("category").HasDefaultValue(UploadFileBucket.GeneralBucket);
        e.HasIndex(x => x.Category).HasDatabaseName("idx_files_category");
        e.Property(x => x.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("now()");
        e.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id").IsRequired();
        e.HasIndex(x => x.UploadedByUserId).HasDatabaseName("idx_files_uploaded_by_user_id");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_files_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_files_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_files_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_files_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_files_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_files_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
