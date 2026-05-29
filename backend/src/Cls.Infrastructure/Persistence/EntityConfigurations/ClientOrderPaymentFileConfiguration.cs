using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class ClientOrderPaymentFileConfiguration : SoftDeletableEntityConfiguration<ClientOrderPaymentFile>
{
    public override void Configure(EntityTypeBuilder<ClientOrderPaymentFile> e)
    {
        base.Configure(e);
        e.ToTable("client_payment_files");
        e.Property(x => x.PaymentId).IsRequired().HasColumnName("client_payment_id");
        e.Property(x => x.FileId).IsRequired().HasColumnName("file_id");
        e.HasIndex(x => x.PaymentId).HasDatabaseName("idx_client_payment_files_payment_id");
        e.HasIndex(x => x.FileId).HasDatabaseName("idx_client_payment_files_file_id");
        e.HasOne(x => x.Payment).WithMany(o => o.Files).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}