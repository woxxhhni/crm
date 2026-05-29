using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderClientPaymentsInfoQuery(int OrderId) : IRequest<OrderPaymentAccountingResponse>;
public class GetOrderClientPaymentsInfoQueryHandler(IUnitOfWork uow, IOrderContractEnricher enricher) : IRequestHandler<GetOrderClientPaymentsInfoQuery, OrderPaymentAccountingResponse>
{
    public async Task<OrderPaymentAccountingResponse> Handle(GetOrderClientPaymentsInfoQuery request, CancellationToken cancellationToken)
    {
        var order = await uow.Orders
            .Query()
            .Include(x => x.Client)
            .Include(x => x.ClientPayments)
                .ThenInclude(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException();
        var result = new OrderPaymentAccountingResponse
        {
            Name = order.Client.Name,
            TotalBalance = order.SellAmount,
            Currency = order.SellCurrency,
        };
        if (!order.ClientPayments.Any())
        {
            await enricher.EnrichTransactionsAsync(result.Transactions, cancellationToken);
            return result;
        }

        result.PaidAmount = order.ClientPayments.Sum(p => p.Amount);
        decimal paidAmount = 0;
        foreach (var payment in order.ClientPayments.OrderBy(x=>x.CreatedAt))
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