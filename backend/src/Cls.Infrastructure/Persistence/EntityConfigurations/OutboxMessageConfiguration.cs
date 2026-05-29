using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cls.Infrastructure.Persistence.EntityConfigurations;
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> e)
    {
        e.ToTable("outbox_messages");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Type).HasColumnName("type").HasMaxLength(200).IsRequired();
        e.Property(x => x.Payload).HasColumnName("payload").IsRequired();
        e.Property(x => x.OccurredOnUtc).HasColumnName("occurred_on_utc").HasDefaultValueSql("now() at time zone 'utc'");
        e.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc");
        e.Property(x => x.Error).HasColumnName("error");
        e.HasIndex(x => x.ProcessedOnUtc).HasDatabaseName("idx_outbox_processed");
    }
}
