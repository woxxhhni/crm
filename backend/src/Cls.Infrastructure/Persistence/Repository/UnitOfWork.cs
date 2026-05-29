
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Abstractions;

namespace Cls.Infrastructure.Persistence.Repository;
public class UnitOfWork(AppDbContext ctx, ICurrentUserService currentUserService) : IUnitOfWork
{
    private IGenericRepository<User>? _users;
    private IGenericRepository<Client>? _clients;
    private IGenericRepository<Provider>? _providers;
    private IGenericRepository<StoredFile>? _storedFiles;
    private IGenericRepository<ClientFileGroup>? _clientFileGroups;
    private IGenericRepository<ClientFileGroupItem>? _clientFileGroupItems;
    private IGenericRepository<ProviderFileGroup>? _providerFileGroups;
    private IGenericRepository<ProviderFileGroupItem>? _providerFileGroupItems;
    private IGenericRepository<Order>? _orders;
    private IGenericRepository<OrderSellInvoice>? _orderSellInvoices;
    private IGenericRepository<OrderBuyInvoice>? _orderBuyInvoices;
    private IGenericRepository<OrderEmployee>? _orderEmployees;
    private IGenericRepository<OrderStageAssignment>? _orderStageAssignments;
    private IGenericRepository<OrderUniqueNumber>? _orderUniqueNumbers;
    private IGenericRepository<Stage>? _stages;
    private IGenericRepository<Step>? _steps;
    private IGenericRepository<OrderSequence>? _orderSequences;
    private IGenericRepository<OrderStepHistory>? _orderStepHistories;
    private IGenericRepository<Note>? _notes;
    private IGenericRepository<OrderLog>? _orderLogs;
    private IGenericRepository<Currency>? _currencies;
    private IGenericRepository<ExtraProvider>? _extraProviders;
    private IGenericRepository<ExtraProviderOrderPayment>? _extraProviderPayments;
    private IGenericRepository<BackupJob>? _backupJobs;


    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(ctx, currentUserService);
    public IGenericRepository<Client> Clients => _clients ??= new GenericRepository<Client>(ctx, currentUserService);
    public IGenericRepository<Provider> Providers => _providers ??= new GenericRepository<Provider>(ctx, currentUserService);
    public IGenericRepository<StoredFile> StoredFiles => _storedFiles ??= new GenericRepository<StoredFile>(ctx, currentUserService);
    public IGenericRepository<ClientFileGroup> ClientFileGroups => _clientFileGroups ??= new GenericRepository<ClientFileGroup>(ctx, currentUserService);
    public IGenericRepository<ClientFileGroupItem> ClientFileGroupItems => _clientFileGroupItems ??= new GenericRepository<ClientFileGroupItem>(ctx, currentUserService);
    public IGenericRepository<ProviderFileGroup> ProviderFileGroups => _providerFileGroups ??= new GenericRepository<ProviderFileGroup>(ctx, currentUserService);
    public IGenericRepository<ProviderFileGroupItem> ProviderFileGroupItems => _providerFileGroupItems ??= new GenericRepository<ProviderFileGroupItem>(ctx, currentUserService);
    public IGenericRepository<Order> Orders => _orders ??= new GenericRepository<Order>(ctx, currentUserService);
    public IGenericRepository<OrderSellInvoice> OrderSellInvoices => _orderSellInvoices ??= new GenericRepository<OrderSellInvoice>(ctx, currentUserService);
    public IGenericRepository<OrderBuyInvoice> OrderBuyInvoices => _orderBuyInvoices ??= new GenericRepository<OrderBuyInvoice>(ctx, currentUserService);
    public IGenericRepository<OrderEmployee> OrderEmployees => _orderEmployees ??= new GenericRepository<OrderEmployee>(ctx, currentUserService);
    public IGenericRepository<OrderStageAssignment> OrderStageAssignments => _orderStageAssignments ??= new GenericRepository<OrderStageAssignment>(ctx, currentUserService);
    public IGenericRepository<OrderUniqueNumber> OrderUniqueNumbers => _orderUniqueNumbers ??= new GenericRepository<OrderUniqueNumber>(ctx, currentUserService);
    public IGenericRepository<Stage> Stages => _stages ??= new GenericRepository<Stage>(ctx, currentUserService);
    public IGenericRepository<Step> Steps => _steps ??= new GenericRepository<Step>(ctx, currentUserService);
    public IGenericRepository<OrderSequence> OrderSequences => _orderSequences ??= new GenericRepository<OrderSequence>(ctx, currentUserService);
    public IGenericRepository<OrderStepHistory> OrderStepHistories => _orderStepHistories ??= new GenericRepository<OrderStepHistory>(ctx, currentUserService);
    public IGenericRepository<Note> Notes => _notes ??= new GenericRepository<Note>(ctx, currentUserService);
    public IGenericRepository<OrderLog> OrderLogs => _orderLogs ??= new GenericRepository<OrderLog>(ctx, currentUserService);
    public IGenericRepository<Currency> Currencies => _currencies ??= new GenericRepository<Currency>(ctx, currentUserService);
    public IGenericRepository<ExtraProvider> ExtraProviders => _extraProviders ??= new GenericRepository<ExtraProvider>(ctx, currentUserService);
    public IGenericRepository<ExtraProviderOrderPayment> ExtraProviderPayments => _extraProviderPayments ??= new GenericRepository<ExtraProviderOrderPayment>(ctx, currentUserService);
    public IGenericRepository<BackupJob> BackupJobs => _backupJobs ??= new GenericRepository<BackupJob>(ctx, currentUserService);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => ctx.SaveChangesAsync(ct);
}
