using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Mapping;
using MediatR;
namespace Cls.Application.Providers.Commands;
public record UpdateProviderCommand(int Id, string Name, string? Phone, string? SecondPhone, string? Email, string? Website, string? Address, string? Description, bool IsActive) : IRequest<Provider?>, IOneWayMap<Provider>;
public class UpdateProviderCommandHandler(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<UpdateProviderCommand, Provider?>
{
    public async Task<Provider?> Handle(UpdateProviderCommand r, CancellationToken ct)
    {
        var e = await uow.Providers.GetByIdAsync(r.Id, ct);
        if (e is null) return null;
        e.Name = r.Name;
        e.Phone = r.Phone;
        e.SecondPhone = r.SecondPhone;
        e.Email = r.Email;
        e.Website = r.Website;
        e.Address = r.Address;
        e.Description = r.Description;
        e.IsActive = r.IsActive;
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedByUserId = currentUserService.UserId;
        await uow.Providers.UpdateAsync(e, ct);
        return e;
    }
}
