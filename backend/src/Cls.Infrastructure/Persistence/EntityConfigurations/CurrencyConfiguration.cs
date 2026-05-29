using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class CurrencyConfiguration : BaseEntityConfiguration<Currency>
{
    public override void Configure(EntityTypeBuilder<Currency> e)
    {
        base.Configure(e);

        e.ToTable("currencies");
        e.Property(x => x.Code).HasColumnName("code").HasMaxLength(5).IsRequired();
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        e.Property(x => x.Symbol).HasColumnName("symbol").HasMaxLength(5).IsRequired();
    }
}
