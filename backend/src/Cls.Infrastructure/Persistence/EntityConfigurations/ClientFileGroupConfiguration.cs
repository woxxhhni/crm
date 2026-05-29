using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class ClientFileGroupConfiguration : SoftDeletableEntityConfiguration<ClientFileGroup>
{
    public override void Configure(EntityTypeBuilder<ClientFileGroup> e)
    {
        base.Configure(e);
        
        e.ToTable("client_file_groups");
        e.Property(x => x.ClientId).HasColumnName("client_id").IsRequired();
        e.HasIndex(x => x.ClientId).HasDatabaseName("idx_client_file_groups_client_id");
        e.HasOne(x => x.Client).WithMany(x=>x.FileGroups).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.Label).HasColumnName("label").HasMaxLength(255).IsRequired();
        e.HasIndex(x => new { x.ClientId, x.Label, x.IsDeleted }).IsUnique().HasDatabaseName("idx_client_file_groups_unique_label");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_client_file_groups_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_client_file_groups_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_client_file_groups_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_client_file_groups_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_client_file_groups_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_client_file_groups_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
