using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderSellInvoiceConfiguration : SoftDeletableEntityConfiguration<OrderSellInvoice>
{
    public override void Configure(EntityTypeBuilder<OrderSellInvoice> e)
    {
        base.Configure(e);

        e.ToTable("order_sell_invoices");
        e.Property(x => x.UploadedAt).HasDefaultValueSql("now()");
        e.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_order_sell_invoices_order_id");
        e.HasOne(x => x.Order).WithMany(o => o.SellInvoices).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
        e.HasIndex(x => x.FileId).HasDatabaseName("idx_order_sell_invoices_file_id");
        e.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
        e.Property(x => x.UploadedAt).HasColumnName("uploaded_at").IsRequired();

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_order_sell_invoices_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_order_sell_invoices_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_order_sell_invoices_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_order_sell_invoices_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_order_sell_invoices_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_order_sell_invoices_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
