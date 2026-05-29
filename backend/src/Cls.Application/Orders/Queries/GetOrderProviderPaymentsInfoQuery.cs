using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderProviderPaymentsInfoQuery(int OrderId) : IRequest<OrderPaymentAccountingResponse>;
public class GetOrderProviderPaymentsInfoQueryHandler(IUnitOfWork uow, IOrderContractEnricher enricher) : IRequestHandler<GetOrderProviderPaymentsInfoQuery, OrderPaymentAccountingResponse>
{
    public async Task<OrderPaymentAccountingResponse> Handle(GetOrderProviderPaymentsInfoQuery request, CancellationToken cancellationToken)
    {
        var order = await uow.Orders
            .Query()
            .Include(x => x.Provider)
            .Include(x => x.ProviderPayments)
                .ThenInclude(x=>x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException();
        var provider = await uow.Providers.GetByIdAsync(order.ProviderId, cancellationToken);
        var result = new OrderPaymentAccountingResponse
        {
            Name = provider.Name,
            TotalBalance = order.BuyAmount,
            Currency = order.BuyCurrency,
        };
        if (!order.ProviderPayments.Any())
        {
            await enricher.EnrichTransactionsAsync(result.Transactions, cancellationToken);
            return result;
        }

        result.PaidAmount = order.ProviderPayments.Sum(p => p.Amount);
     
        decimal paidAmount = 0;
        foreach (var payment in order.ProviderPayments.OrderBy(x => x.CreatedAt))
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