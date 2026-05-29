using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class ExtraProviderOrderPaymentConfiguration : SoftDeletableEntityConfiguration<ExtraProviderOrderPayment>
{
    public override void Configure(EntityTypeBuilder<ExtraProviderOrderPayment> e)
    {
        base.Configure(e);
        e.ToTable("extra_provider_payments");

        e.Property(x => x.ExtraProviderId).IsRequired().HasColumnName("extra_provider_id");
        e.Property(x => x.Amount).IsRequired().HasPrecision(15, 2).HasColumnName("amount");
        e.Property(x => x.OrderId).HasColumnName("order_id");
        e.Property(x => x.PaymentType).IsRequired().HasColumnName("payment_type");
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.PaymentDate).IsRequired().HasColumnName("payment_date");

        e.HasIndex(x => x.ExtraProviderId).HasDatabaseName("idx_extra_provider_payments_extra_provider_id");
        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_extra_provider_payments_order_id");
        e.HasIndex(x => x.PaymentDate).HasDatabaseName("idx_extra_provider_payments_payment_date");
        e.HasIndex(x => x.PaymentType).HasDatabaseName("idx_extra_provider_payments_payment_type");

        e.HasOne(x => x.ExtraProvider).WithMany(p => p.Payments).HasForeignKey(x => x.ExtraProviderId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
