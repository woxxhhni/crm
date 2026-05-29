using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class ClientOrderPaymentConfiguration : SoftDeletableEntityConfiguration<ClientOrderPayment>
{
    public override void Configure(EntityTypeBuilder<ClientOrderPayment> e)
    {
        base.Configure(e);

        e.ToTable("client_payments");

        e.Property(x => x.Amount).IsRequired().HasPrecision(15, 2).HasColumnName("amount");
        e.Property(x => x.OrderId).HasColumnName("order_id");
        e.Property(x => x.PaymentType).IsRequired().HasColumnName("payment_type");
        e.Property(x => x.Description).HasColumnName("description");
        e.Property(x => x.PaymentDate).IsRequired().HasColumnName("payment_date");
       

        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_client_payments_order_id");
        e.HasIndex(x => x.PaymentDate).HasDatabaseName("idx_client_payments_payment_date");
        e.HasIndex(x => x.PaymentType).HasDatabaseName("idx_client_payments_payment_type");
        e.HasIndex(x => new { x.OrderId, x.PaymentDate }).HasDatabaseName("idx_client_payments_order_date");

        e.HasOne(x => x.Order).WithMany(o => o.ClientPayments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);

    }
}