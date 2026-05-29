using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderConfiguration : SoftDeletableEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> e)
    {
        base.Configure(e);

        e.ToTable("orders", tb =>
        tb.HasCheckConstraint(
            "CK_orders_order_date_first_action",
            "(\"first_action_date\" IS NULL) OR (\"order_date\" <= \"first_action_date\")"
        ));
        e.Property(o => o.OrderNumber).HasColumnName("order_number").HasMaxLength(100).IsRequired();
        e.HasIndex(o => o.OrderNumber).IsUnique().HasDatabaseName("idx_orders_order_number");
        e.Property(o => o.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
        e.Property(o => o.OrderDate).HasColumnName("order_date").IsRequired();
        e.HasIndex(o => o.OrderDate).HasDatabaseName("idx_orders_order_date");
        e.Property(o => o.Description).HasColumnName("description");
        e.Property(o => o.BuyCurrency).HasColumnName("buy_currency").HasMaxLength(10).IsRequired();
        e.Property(o => o.BuyAmount).HasColumnName("buy_amount").HasPrecision(18, 2);
        e.Property(o => o.SellCurrency).HasColumnName("sell_currency").HasMaxLength(10).IsRequired();
        e.Property(o => o.SellAmount).HasColumnName("sell_amount").HasPrecision(18, 2);
        e.Property(o => o.ClientBalance).HasColumnName("client_balance").HasPrecision(18, 2);
        e.Property(o => o.ProviderBalance).HasColumnName("provider_balance").HasPrecision(18, 2);
        e.Property(o => o.BalancesLastCalculatedAt).HasColumnName("balances_last_calculated_at");
        e.Property(o => o.ClientId).HasColumnName("client_id").IsRequired();
        e.HasIndex(o => o.ClientId).HasDatabaseName("idx_orders_client_id");
        e.HasOne(o => o.Client).WithMany().HasForeignKey(o => o.ClientId).OnDelete(DeleteBehavior.Restrict);
        e.Property(o => o.ProviderId).HasColumnName("provider_id").IsRequired();
        e.HasIndex(o => o.ProviderId).HasDatabaseName("idx_orders_provider_id");
        e.HasOne(o => o.Provider).WithMany().HasForeignKey(o => o.ProviderId).OnDelete(DeleteBehavior.Restrict);
        e.Property(o => o.CurrentStepId).HasColumnName("current_step_id").IsRequired();
        e.HasIndex(o => o.CurrentStepId).HasDatabaseName("idx_orders_current_step_id");
        e.HasOne(o => o.CurrentStep).WithMany().HasForeignKey(o => o.CurrentStepId).OnDelete(DeleteBehavior.Restrict);
        e.Property(o => o.Status).HasColumnName("status").IsRequired();
        e.HasIndex(o => o.Status).HasDatabaseName("idx_orders_status");
        e.Property(o => o.FirstActionDate).HasColumnName("first_action_date");
        e.Property(o => o.CompletedAt).HasColumnName("completed_at");
        e.Property(o => o.CanceledAt).HasColumnName("canceled_at");
        e.Property(o => o.SuspendedAt).HasColumnName("suspended_at");
        e.HasIndex(o => new { o.Status, o.OrderDate }).HasDatabaseName("idx_orders_status_date");
        e.HasIndex(o => new { o.OrderDate, o.SellAmount }).HasDatabaseName("idx_orders_date_amount");
        e.HasIndex(o => new { o.CreatedByUserId, o.OrderDate }).HasDatabaseName("idx_orders_user_date");

        // SoftDelete & BaseEntity Relations and Indexes
        //e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_orders_created_at");
        //e.HasIndex(x => x.CreatedByUserId).HasDatabaseName("idx_orders_created_by_user_id");
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.UpdatedAt).HasDatabaseName("idx_orders_updated_at");
        //e.HasIndex(x => x.UpdatedByUserId).HasDatabaseName("idx_orders_updated_by_user_id");
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        //e.HasIndex(x => x.DeletedAt).HasDatabaseName("idx_orders_deleted_at");
        //e.HasIndex(x => x.DeletedByUserId).HasDatabaseName("idx_orders_deleted_by_user_id");
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
