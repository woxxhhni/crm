using Cls.Application.Clients.Commands;
using Cls.Application.Providers.Commands;
using Cls.Domain.Enums;
using MediatR;

namespace Cls.Api.Services;

public interface IProfilePictureOrchestrator
{
    Task ApplyClientProfileAsync(int clientId, int userId, IFormFile? file, CancellationToken ct);
    Task ApplyProviderProfileAsync(int providerId, int userId, IFormFile? file, CancellationToken ct);
}

public sealed class ProfilePictureOrchestrator(IMediator mediator, IFileService fileService) : IProfilePictureOrchestrator
{
    public async Task ApplyClientProfileAsync(int clientId, int userId, IFormFile? file, CancellationToken ct)
    {
        if (file is not { Length: > 0 })
            return;

        var fileId = await fileService.UploadFile(userId, file, UploadFileBucket.ClientProfile, ct);
        await mediator.Send(new SetClientProfilePictureCommand(clientId, fileId), ct);
    }

    public async Task ApplyProviderProfileAsync(int providerId, int userId, IFormFile? file, CancellationToken ct)
    {
        if (file is not { Length: > 0 })
            return;

        var fileId = await fileService.UploadFile(userId, file, UploadFileBucket.ProviderProfile, ct);
        await mediator.Send(new SetProviderProfilePictureCommand(providerId, fileId), ct);
    }
}
