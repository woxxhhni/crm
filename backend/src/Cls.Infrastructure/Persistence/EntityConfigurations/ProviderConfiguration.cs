using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class ProviderConfiguration : SoftDeletableEntityConfiguration<Provider>
{
    public override void Configure(EntityTypeBuilder<Provider> e)
    {
        base.Configure(e);

        e.ToTable("providers");
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        e.HasIndex(x => x.Name).HasDatabaseName("idx_providers_name");
        e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        e.Property(x => x.SecondPhone).HasColumnName("second_phone").HasMaxLength(50);
        e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
        e.HasIndex(x => x.Email).HasDatabaseName("idx_providers_email");
        e.Property(x => x.Website).HasColumnName("website").HasMaxLength(255);
        e.Property(x => x.Address).HasColumnName("address");
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        e.HasIndex(x => x.IsActive).HasDatabaseName("idx_providers_is_active");
        e.Property(x => x.ProfileFileId).HasColumnName("profile_file_id");
        e.HasOne<StoredFile>().WithMany().HasForeignKey(x => x.ProfileFileId).OnDelete(DeleteBehavior.SetNull);

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_providers_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_providers_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_providers_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_providers_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_providers_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_providers_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
