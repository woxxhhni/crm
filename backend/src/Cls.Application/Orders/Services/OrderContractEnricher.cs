using Cls.Application.Files;
using Cls.Shared.Contracts.Orders;
using MediatR;

namespace Cls.Application.Orders.Services;

public interface IOrderContractEnricher
{
    Task EnrichTransactionsAsync(IEnumerable<OrderPaymentAccountingResponse.OrderPaymentTransactionResponse> transactions, CancellationToken ct);
    Task EnrichPaymentAsync(OrderPaymentResponse payment, CancellationToken ct);
    Task EnrichNotesAsync(IEnumerable<OrderNoteListResponse> notes, CancellationToken ct);
    Task EnrichNoteAsync(OrderNoteResponse note, CancellationToken ct);
}

public sealed class OrderContractEnricher(IMediator mediator) : IOrderContractEnricher
{
    private static readonly TimeSpan UrlLifetime = TimeSpan.FromHours(1);

    public async Task EnrichTransactionsAsync(
        IEnumerable<OrderPaymentAccountingResponse.OrderPaymentTransactionResponse> transactions,
        CancellationToken ct)
    {
        foreach (var transaction in transactions.Where(x => x.UserProfileFileId.HasValue))
        {
            transaction.UserProfileUrl = await GetUrlAsync(transaction.UserProfileFileId!.Value, ct);
        }
    }

    public async Task EnrichPaymentAsync(OrderPaymentResponse payment, CancellationToken ct)
    {
        foreach (var file in payment.Files)
            file.Url = await GetUrlAsync(file.Id, ct);

        if (payment.UserProfileFileId.HasValue)
            payment.UserProfileUrl = await GetUrlAsync(payment.UserProfileFileId.Value, ct);
    }

    public async Task EnrichNotesAsync(IEnumerable<OrderNoteListResponse> notes, CancellationToken ct)
    {
        foreach (var note in notes)
        {
            foreach (var file in note.Files)
                file.Url = await GetUrlAsync(file.Id, ct);

            if (note.UserProfileFileId.HasValue)
                note.UserProfileUrl = await GetUrlAsync(note.UserProfileFileId.Value, ct);
        }
    }

    public async Task EnrichNoteAsync(OrderNoteResponse note, CancellationToken ct)
    {
        foreach (var file in note.Files)
            file.Url = await GetUrlAsync(file.Id, ct);

        if (note.UserProfileFileId.HasValue)
            note.UserProfileUrl = await GetUrlAsync(note.UserProfileFileId.Value, ct);
    }

    private Task<string> GetUrlAsync(int fileId, CancellationToken ct) =>
        mediator.Send(new GetDownloadUrlQuery(fileId, UrlLifetime), ct);
}
