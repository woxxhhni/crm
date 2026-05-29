using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class BackupJobConfiguration : BaseEntityConfiguration<BackupJob>
{
    public override void Configure(EntityTypeBuilder<BackupJob> e)
    {
        base.Configure(e);

        e.ToTable("backup_jobs");

        e.Property(x => x.Status).HasColumnName("status").HasDefaultValue(BackupJobStatus.Pending);
        e.Property(x => x.Type).HasColumnName("type");
        e.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255);
        e.Property(x => x.FilePath).HasColumnName("file_path").HasMaxLength(500);
        e.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes");
        e.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(4000);
        e.Property(x => x.StartedAt).HasColumnName("started_at");
        e.Property(x => x.CompletedAt).HasColumnName("completed_at");
        e.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id").IsRequired();

        e.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => x.Status).HasDatabaseName("idx_backup_jobs_status");
        e.HasIndex(x => x.Type).HasDatabaseName("idx_backup_jobs_type");
    }
}
