using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class ProviderFileGroupConfiguration : SoftDeletableEntityConfiguration<ProviderFileGroup>
{
    public override void Configure(EntityTypeBuilder<ProviderFileGroup> e)
    {
        base.Configure(e);

        e.ToTable("provider_file_groups");
        e.Property(x => x.ProviderId).HasColumnName("provider_id").IsRequired();
        e.HasIndex(x => x.ProviderId).HasDatabaseName("idx_provider_file_groups_provider_id");
        e.HasOne(x => x.Provider).WithMany(x=>x.FileGroups).HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.Label).HasColumnName("label").HasMaxLength(255).IsRequired();
        e.HasIndex(x => new { x.ProviderId, x.Label, x.IsDeleted }).IsUnique().HasDatabaseName("idx_provider_file_groups_unique_label");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_provider_file_groups_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_provider_file_groups_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_provider_file_groups_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_provider_file_groups_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_provider_file_groups_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_provider_file_groups_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
