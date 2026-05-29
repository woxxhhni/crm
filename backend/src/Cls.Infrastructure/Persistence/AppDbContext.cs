using Cls.Domain.Entities;
using Cls.Shared.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace Cls.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Provider> Providers => Set<Provider>();
        public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
        public DbSet<ClientFileGroup> ClientFileGroups => Set<ClientFileGroup>();
        public DbSet<ClientFileGroupItem> ClientFileGroupItems => Set<ClientFileGroupItem>();
        public DbSet<ProviderFileGroup> ProviderFileGroups => Set<ProviderFileGroup>();
        public DbSet<ProviderFileGroupItem> ProviderFileGroupItems => Set<ProviderFileGroupItem>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderSellInvoice> OrderSellInvoices => Set<OrderSellInvoice>();
        public DbSet<OrderBuyInvoice> OrderBuyInvoices => Set<OrderBuyInvoice>();
        public DbSet<OrderEmployee> OrderEmployees => Set<OrderEmployee>();
        public DbSet<OrderStageAssignment> OrderStageAssignments => Set<OrderStageAssignment>();
        public DbSet<OrderUniqueNumber> OrderUniqueNumbers => Set<OrderUniqueNumber>();
        public DbSet<ExtraProvider> ExtraProviders => Set<ExtraProvider>();

        public DbSet<Stage> Stages => Set<Stage>();
        public DbSet<Step> Steps => Set<Step>();
        public DbSet<OrderSequence> OrderSequences => Set<OrderSequence>();
        public DbSet<OrderStepHistory> OrderStepHistory => Set<OrderStepHistory>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<OrderLog> OrderLogs => Set<OrderLog>();
        public DbSet<NoteFile> NoteFiles => Set<NoteFile>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<BackupJob> BackupJobs => Set<BackupJob>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                    var body = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(lambda);
                }
            }
        }
    }
}
