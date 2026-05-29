using Cls.Domain.Entities;
namespace Cls.Application.Abstractions;
public interface IUnitOfWork
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Client> Clients { get; }
    IGenericRepository<Provider> Providers { get; }
    IGenericRepository<StoredFile> StoredFiles { get; }
    IGenericRepository<ClientFileGroup> ClientFileGroups { get; }
    IGenericRepository<ClientFileGroupItem> ClientFileGroupItems { get; }
    IGenericRepository<ProviderFileGroup> ProviderFileGroups { get; }
    IGenericRepository<ProviderFileGroupItem> ProviderFileGroupItems { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderSellInvoice> OrderSellInvoices { get; }
    IGenericRepository<OrderBuyInvoice> OrderBuyInvoices { get; }
    IGenericRepository<OrderEmployee> OrderEmployees { get; }
    IGenericRepository<OrderStageAssignment> OrderStageAssignments { get; }
    IGenericRepository<OrderUniqueNumber> OrderUniqueNumbers { get; }
    IGenericRepository<Stage> Stages { get; }
    IGenericRepository<Step> Steps { get; }
    IGenericRepository<OrderSequence> OrderSequences { get; }
    IGenericRepository<OrderStepHistory> OrderStepHistories { get; }
    IGenericRepository<Note> Notes { get; }
    IGenericRepository<OrderLog> OrderLogs { get; }
    IGenericRepository<ExtraProvider> ExtraProviders { get; }
    IGenericRepository<ExtraProviderOrderPayment> ExtraProviderPayments { get; }
    IGenericRepository<Currency> Currencies { get; }
    IGenericRepository<BackupJob> BackupJobs { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
