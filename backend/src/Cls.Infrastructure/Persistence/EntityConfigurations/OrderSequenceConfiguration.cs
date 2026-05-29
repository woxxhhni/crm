using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class OrderSequenceConfiguration : SoftDeletableEntityConfiguration<OrderSequence>
{
    public override void Configure(EntityTypeBuilder<OrderSequence> e)
    {
        base.Configure(e);

        e.ToTable("order_sequences");
        e.Property(x => x.Year).HasColumnName("year").IsRequired();
        e.Property(x => x.LastNumber).HasColumnName("last_number").IsRequired().HasDefaultValue(0);
        e.Property(x => x.Prefix).HasColumnName("prefix").HasMaxLength(20).IsRequired().HasDefaultValue("ORD");
        e.HasIndex(x => x.Year).IsUnique().HasDatabaseName("idx_order_sequences_year");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_order_sequences_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_order_sequences_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_order_sequences_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_order_sequences_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_order_sequences_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_order_sequences_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
