using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class ClientConfiguration : SoftDeletableEntityConfiguration<Client>
{
    public override void Configure(EntityTypeBuilder<Client> e)
    {
        base.Configure(e);

        e.ToTable("clients");
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        e.HasIndex(x => x.Name).HasDatabaseName("idx_clients_name");
        e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        e.Property(x => x.SecondPhone).HasColumnName("second_phone").HasMaxLength(50);
        e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
        e.HasIndex(x => x.Email).HasDatabaseName("idx_clients_email");
        e.Property(x => x.Website).HasColumnName("website").HasMaxLength(255);
        e.Property(x => x.Address).HasColumnName("address");
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        e.HasIndex(x => x.IsActive).HasDatabaseName("idx_clients_is_active");
        e.Property(x => x.ProfileFileId).HasColumnName("profile_file_id");
        e.HasOne<StoredFile>().WithMany().HasForeignKey(x => x.ProfileFileId).OnDelete(DeleteBehavior.SetNull);

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_clients_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_clients_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_clients_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_clients_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_clients_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_clients_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
