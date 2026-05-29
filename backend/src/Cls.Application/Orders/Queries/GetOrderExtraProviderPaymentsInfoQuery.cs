using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderExtraProviderPaymentsInfoQuery(int OrderId, int ExtraProviderId) : IRequest<OrderPaymentAccountingResponse>;

public class GetOrderExtraProviderPaymentsInfoQueryHandler(IUnitOfWork uow, IOrderContractEnricher enricher) : IRequestHandler<GetOrderExtraProviderPaymentsInfoQuery, OrderPaymentAccountingResponse>
{
    public async Task<OrderPaymentAccountingResponse> Handle(GetOrderExtraProviderPaymentsInfoQuery request, CancellationToken cancellationToken)
    {
        var ep = await uow.ExtraProviders
            .Query()
            .Include(x => x.Provider)
            .Include(x => x.Payments)
                .ThenInclude(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.ExtraProviderId, cancellationToken);

        if (ep == null) throw new NotFoundException();
        if (ep.OrderId != request.OrderId) throw new InvalidActionException();

        var result = new OrderPaymentAccountingResponse
        {
            Name = ep.Provider.Name,
            TotalBalance = ep.Amount,
            Currency = ep.Currency
        };

        if (!ep.Payments.Any())
        {
            await enricher.EnrichTransactionsAsync(result.Transactions, cancellationToken);
            return result;
        }

        result.PaidAmount = ep.Payments.Sum(p => p.Amount);

        decimal paidAmount = 0;
        foreach (var payment in ep.Payments.OrderBy(x => x.CreatedAt))
        {
            result.Transactions.Add(new OrderPaymentAccountingResponse.OrderPaymentTransactionResponse
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentType = payment.PaymentType.ToString(),
                CreatedAt = payment.CreatedAt,
                RemainingAmount = result.TotalBalance - paidAmount - payment.Amount,
                UserFullName = payment.CreatedByUser.Name,
                UserProfileFileId = payment.CreatedByUser.FileId,
                UserProfileUrl = ""
            });
            paidAmount += payment.Amount;
        }
        result.Transactions = result.Transactions.OrderByDescending(x => x.CreatedAt).ToList();
        await enricher.EnrichTransactionsAsync(result.Transactions, cancellationToken);
        return result;
    }
}
