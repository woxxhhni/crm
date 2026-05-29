using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderClientPaymentByIdQuery(int OrderId, int Id) : IRequest<OrderPaymentResponse>;
public class GetOrderClientPaymentByIdQueryHandler(IUnitOfWork uow, IOrderContractEnricher enricher) : IRequestHandler<GetOrderClientPaymentByIdQuery, OrderPaymentResponse>
{
    public async Task<OrderPaymentResponse> Handle(GetOrderClientPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await uow.Orders
            .Query()
            .Include(x => x.ClientPayments)
                .ThenInclude(x=>x.Files)
                .ThenInclude(x=>x.File)
            .Include(x => x.ClientPayments)
                .ThenInclude(x=>x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException("Order not found");
        var payment = order.ClientPayments.FirstOrDefault(x => x.Id == request.Id);
        if(payment == null)
            throw new NotFoundException("Payment not found");
        var response = new OrderPaymentResponse
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaymentType = payment.PaymentType.ToString(),
            Description = payment.Description,
            CreatedAt = payment.CreatedAt,
            Files = payment.Files.Select(a => new FileResponse
            {
                Id = a.FileId,
                Name = a.File.OriginalFilename,
            }).ToList(),
            UserFullName = payment.CreatedByUser.Name,
            UserProfileFileId = payment.CreatedByUser.FileId
        };
        await enricher.EnrichPaymentAsync(response, cancellationToken);
        return response;
    }
}