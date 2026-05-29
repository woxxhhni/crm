using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderUniqueNumberConfiguration : SoftDeletableEntityConfiguration<OrderUniqueNumber>
{
    public override void Configure(EntityTypeBuilder<OrderUniqueNumber> e)
    {
        base.Configure(e);

        e.ToTable("order_unique_numbers");
        e.Property(x => x.Label).HasColumnName("label").HasMaxLength(255).IsRequired();
        e.HasIndex(x => x.Label).HasDatabaseName("idx_order_unique_numbers_label");
        e.Property(x => x.Value).HasColumnName("value").HasMaxLength(255).IsRequired();
        e.HasIndex(x => x.Value).HasDatabaseName("idx_order_unique_numbers_value");
        e.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_order_unique_numbers_order_id");
        e.HasOne(x => x.Order).WithMany(o => o.UniqueNumbers).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.HasIndex(x => new { x.OrderId, x.Label }).IsUnique().HasDatabaseName("idx_order_unique_numbers_unique_label");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_order_unique_numbers_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_order_unique_numbers_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_order_unique_numbers_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_order_unique_numbers_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_order_unique_numbers_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_order_unique_numbers_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
