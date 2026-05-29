using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;

public class OrderEmployeeConfiguration : SoftDeletableEntityConfiguration<OrderEmployee>
{
    public override void Configure(EntityTypeBuilder<OrderEmployee> e)
    {
        base.Configure(e);

        e.ToTable("order_employees");
        e.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        e.HasIndex(x => x.OrderId).HasDatabaseName("idx_order_employees_order_id");
        e.HasOne(x => x.Order).WithMany(o => o.Employees).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        e.HasIndex(x => x.UserId).HasDatabaseName("idx_order_employees_user_id");

        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
