using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Mapping;
using MediatR;
namespace Cls.Application.Clients.Commands;
public record CreateClientCommand(string Name, string? Phone, string? SecondPhone, string? Email, string? Website, string? Address, string? Description, bool IsActive) : IRequest<Client>, IOneWayMap<Client>;
public class CreateClientCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<CreateClientCommand, Client>
{
    public async Task<Client> Handle(CreateClientCommand r, CancellationToken ct)
    {
        var e = mapper.Map<Client>(r);
        var added = await uow.Clients.AddAsync(e, ct);
        return added;
    }
}
