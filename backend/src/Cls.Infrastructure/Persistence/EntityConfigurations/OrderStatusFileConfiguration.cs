using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderStatusFileConfiguration : SoftDeletableEntityConfiguration<OrderStatusFile>
{
    public override void Configure(EntityTypeBuilder<OrderStatusFile> e)
    {
        base.Configure(e);
        e.ToTable("order_status_files");
        e.Property(osf => osf.OrderId).HasColumnName("order_id").IsRequired();
        e.Property(osf => osf.OrderStatus).HasColumnName("order_status").IsRequired();
        e.Property(osf => osf.FileId).HasColumnName("file_id").IsRequired();

        e.HasIndex(osf => osf.OrderId).HasDatabaseName("idx_order_status_files_order_id");
        e.HasIndex(osf => osf.FileId).HasDatabaseName("idx_order_status_files_file_id");

        e.HasOne(osf => osf.Order).WithMany(o => o.StatusFiles).HasForeignKey(osf => osf.OrderId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(osf => osf.File).WithMany().HasForeignKey(osf => osf.FileId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}