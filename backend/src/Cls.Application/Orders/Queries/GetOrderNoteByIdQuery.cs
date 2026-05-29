using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderNoteByIdQuery(int OrderId, int Id) : IRequest<OrderNoteResponse>;
public class GetOrderNoteByIdQueryHandler(IUnitOfWork uow, ICurrentUserService currentUserService, IOrderContractEnricher enricher) : IRequestHandler<GetOrderNoteByIdQuery, OrderNoteResponse>
{
    public async Task<OrderNoteResponse> Handle(GetOrderNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await uow.Orders
            .Query()
            .Include(x=>x.Employees)
            .Include(x=>x.StageAssignments)
            .Include(x=>x.Client)
            .Include(x=>x.Provider)
            .Include(x => x.Notes)
                .ThenInclude(x => x.Files)
                .ThenInclude(x => x.File)
            .Include(x=>x.Notes)
                .ThenInclude(x=>x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException("Order not found");

        if (currentUserService.Role == UserRole.Employee
            && !OrderAccess.IsAssignedToUser(order, currentUserService.UserId))
            throw new InvalidAccessException();

        var note = order.Notes.FirstOrDefault(x => x.Id == request.Id);
        if (note == null)
            throw new NotFoundException("Note not found");
        var response = new OrderNoteResponse
        {
            Id = note.Id,
            Date = note.NoteDate,
            Title = note.Title,
            Description = note.Description,
            CreatedAt = note.CreatedAt,
            UserFullName = note.CreatedByUser.Name,
            UserProfileFileId = note.CreatedByUser.FileId,
            ClientName = order.Client.Name,
            ProviderName = order.Provider.Name,
            Files = note.Files.Select(a => new FileResponse
            {
                Id = a.FileId,
                Name = a.File.OriginalFilename,
            }).ToList(),
        };
        await enricher.EnrichNoteAsync(response, cancellationToken);
        return response;
    }
}
