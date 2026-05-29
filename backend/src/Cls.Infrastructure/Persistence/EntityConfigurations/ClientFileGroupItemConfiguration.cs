using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class ClientFileGroupItemConfiguration : SoftDeletableEntityConfiguration<ClientFileGroupItem>
{
    public override void Configure(EntityTypeBuilder<ClientFileGroupItem> e)
    {
        base.Configure(e);

        e.ToTable("client_file_group_items");
        e.Property(x => x.ClientFileGroupId).HasColumnName("client_file_group_id").IsRequired();
        e.HasIndex(e => e.ClientFileGroupId).HasDatabaseName("idx_client_file_group_items_group_id");
        e.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
        e.HasIndex(e => e.FileId).HasDatabaseName("idx_client_file_group_items_file_id");
        e.HasOne(e => e.Group).WithMany(g => g.Items).HasForeignKey(e => e.ClientFileGroupId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(e => e.File).WithMany().HasForeignKey(e => e.FileId).OnDelete(DeleteBehavior.Cascade);

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_client_file_group_items_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_client_file_group_items_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_client_file_group_items_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_client_file_group_items_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_client_file_group_items_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_client_file_group_items_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
