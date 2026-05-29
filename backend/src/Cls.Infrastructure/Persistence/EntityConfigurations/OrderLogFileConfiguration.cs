using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderLogFileConfiguration : BaseEntityConfiguration<OrderLogFile>
{
    public override void Configure(EntityTypeBuilder<OrderLogFile> e)
    {
        base.Configure(e);

        e.ToTable("order_log_files");

        e.Property(x => x.OrderLogId).HasColumnName("order_log_id").IsRequired();
        e.HasIndex(x => x.OrderLogId).HasDatabaseName("idx_order_log_files_order_log_id");
        e.HasOne(x => x.OrderLog).WithMany(ol => ol.Files).HasForeignKey(x => x.OrderLogId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
        e.HasIndex(x => x.FileId).HasDatabaseName("idx_order_log_files_file_id");
        e.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
    }
}