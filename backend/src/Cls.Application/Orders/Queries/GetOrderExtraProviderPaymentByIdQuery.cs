using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Shared.Contracts.Common;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderExtraProviderPaymentByIdQuery(int OrderId, int PaymentId) : IRequest<OrderPaymentResponse>;

public class GetOrderExtraProviderPaymentByIdQueryHandler(IUnitOfWork uow, IOrderContractEnricher enricher) : IRequestHandler<GetOrderExtraProviderPaymentByIdQuery, OrderPaymentResponse>
{
    public async Task<OrderPaymentResponse> Handle(GetOrderExtraProviderPaymentByIdQuery r, CancellationToken ct)
    {
        var payment = await uow.ExtraProviderPayments.Query()
            .Include(x => x.Files).ThenInclude(x => x.File)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == r.PaymentId && x.OrderId == r.OrderId, ct);

        if (payment == null) throw new NotFoundException();

        var response = new OrderPaymentResponse
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaymentType = payment.PaymentType.ToString(),
            Description = payment.Description,
            UserFullName = payment.CreatedByUser.Name,
            UserProfileFileId = payment.CreatedByUser.FileId,
            CreatedAt = payment.CreatedAt,
            Files = payment.Files.Select(f => new FileResponse { Id = f.FileId, Name = f.File.OriginalFilename }).ToList()
        };
        await enricher.EnrichPaymentAsync(response, ct);
        return response;
    }
}
