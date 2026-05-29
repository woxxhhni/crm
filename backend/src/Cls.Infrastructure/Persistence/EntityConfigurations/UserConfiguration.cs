using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class UserConfiguration : SoftDeletableEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> e)
    {
        base.Configure(e);

        e.ToTable("users");
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("idx_users_email");
        e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        e.Property(x => x.Address).HasColumnName("address");
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(32).HasDefaultValue(UserRole.Employee);
        e.HasIndex(x => x.Role).HasDatabaseName("idx_users_role");
        e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        e.HasIndex(x => x.IsActive).HasDatabaseName("idx_users_is_active");
        e.HasIndex(x => new { x.IsActive, x.Role }).HasDatabaseName("idx_users_active_role");
        e.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        e.Property(x => x.FileId).HasColumnName("file_id");
        e.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_users_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_users_created_by_user_id");
        //e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_users_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_users_updated_by_user_id");
        //e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_users_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_users_deleted_by_user_id");
        //e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
