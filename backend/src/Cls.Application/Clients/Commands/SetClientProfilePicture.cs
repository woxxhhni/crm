using Cls.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Clients.Commands;

public record SetClientProfilePictureCommand(int ClientId, int? FileId) : IRequest;

public class SetClientProfilePictureHandler(IUnitOfWork uow) : IRequestHandler<SetClientProfilePictureCommand>
{    
    
public async Task Handle(SetClientProfilePictureCommand request, CancellationToken ct)
{
    var client = await uow.Clients.GetByIdAsync(request.ClientId, ct)
        ?? throw new KeyNotFoundException("Client not found");
    client.SetProfileFile(request.FileId);
    await uow.Clients.UpdateAsync(client, ct);
}

}
