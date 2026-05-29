using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Mapping;
using MediatR;
namespace Cls.Application.Clients.Commands;
public record UpdateClientCommand(int Id, string Name, string? Phone, string? SecondPhone, string? Email, string? Website, string? Address, string? Description, bool IsActive) : IRequest<Client?>, IOneWayMap<Client>;
public class UpdateClientCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<UpdateClientCommand, Client?>
{   
    public async Task<Client?> Handle(UpdateClientCommand r, CancellationToken ct)
    {
        var e = await uow.Clients.GetByIdAsync(r.Id, ct);
        if (e is null) return null;
        mapper.Map(r, e);
        e.UpdatedAt = DateTime.UtcNow;
        await uow.Clients.UpdateAsync(e, ct);
        return e;
    }
}

