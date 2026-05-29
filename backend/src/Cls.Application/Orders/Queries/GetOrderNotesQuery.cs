using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderNotesQuery(int OrderId) : IRequest<List<OrderNoteListResponse>>;
public class GetOrderNotesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUserService, IOrderContractEnricher enricher) : IRequestHandler<GetOrderNotesQuery, List<OrderNoteListResponse>>
{
    public async Task<List<OrderNoteListResponse>> Handle(GetOrderNotesQuery request, CancellationToken cancellationToken)
    {
        var order = await uow.Orders
            .Query()
            .Include(x => x.Employees)
            .Include(x => x.StageAssignments)
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .Include(x => x.Notes)
                .ThenInclude(x => x.Files)
                    .ThenInclude(x => x.File)
            .Include(x => x.Notes)
                .ThenInclude(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException();

        if (currentUserService.Role == UserRole.Employee
            && !OrderAccess.IsAssignedToUser(order, currentUserService.UserId))
            throw new InvalidAccessException();

        var notes = order.Notes
            .Select(note => new OrderNoteListResponse
            {
                Id = note.Id,
                Date = note.NoteDate,
                Title = note.Title,
                UserFullName = note.CreatedByUser.Name,
                UserProfileFileId = note.CreatedByUser.FileId,
                ClientName = order.Client.Name,
                ProviderName = order.Provider.Name,
                Files = note.Files.Select(a => new FileResponse
                {
                    Id = a.Id,
                    Name = a.File.OriginalFilename,
                }).ToList()
            })
            .ToList();

        await enricher.EnrichNotesAsync(notes, cancellationToken);
        return notes;
    }
}
