using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class ExtraProviderConfiguration : SoftDeletableEntityConfiguration<ExtraProvider>
{
    public override void Configure(EntityTypeBuilder<ExtraProvider> e)
    {
        base.Configure(e);
        e.ToTable("extra_providers");

        e.Property(x => x.OrderId).IsRequired().HasColumnName("order_id");
        e.Property(x => x.ProviderId).IsRequired().HasColumnName("provider_id");
        e.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);
        e.Property(x => x.Currency).IsRequired().HasMaxLength(3).HasColumnName("currency");

        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_extra_providers_order_id");
        e.HasIndex(x => x.ProviderId).HasDatabaseName("idx_extra_providers_provider_id");

        e.HasOne(x => x.Order).WithMany(x => x.ExtraProviders).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Provider).WithMany().HasForeignKey(x => x.ProviderId).OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
