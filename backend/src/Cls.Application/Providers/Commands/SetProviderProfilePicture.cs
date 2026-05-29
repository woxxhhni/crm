using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;

namespace Cls.Application.Providers.Commands;

public record SetProviderProfilePictureCommand(int ProviderId, int? FileId) : IRequest<Provider?>;

public class SetProviderProfilePictureHandler(IUnitOfWork uow) : IRequestHandler<SetProviderProfilePictureCommand, Provider?>
{
    public async Task<Provider?> Handle(SetProviderProfilePictureCommand r, CancellationToken ct)
    {
        var e = await uow.Providers.GetByIdAsync(r.ProviderId, ct);
        if (e is null) return null;

        // Domain method (encapsulated)
        e.SetProfileFile(r.FileId);

        // Keep parity with your Update handler
        e.UpdatedAt = DateTime.UtcNow;

        await uow.Providers.UpdateAsync(e, ct);

        return e;
    }
}
