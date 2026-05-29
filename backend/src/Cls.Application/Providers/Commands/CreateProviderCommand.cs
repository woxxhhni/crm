
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Mapping;
using MediatR;
namespace Cls.Application.Providers.Commands;
public record CreateProviderCommand(string Name, string? Phone, string? SecondPhone, string? Email, string? Website, string? Address, string? Description, bool IsActive) : IRequest<Provider>, IOneWayMap<Provider>;
public class CreateProviderCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateProviderCommand, Provider>
{
    public async Task<Provider> Handle(CreateProviderCommand r, CancellationToken ct)
    {
        var e = new Provider { Name = r.Name, Phone = r.Phone, SecondPhone = r.SecondPhone, Email = r.Email, Website = r.Website, Address = r.Address, Description = r.Description, IsActive = r.IsActive, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var added = await uow.Providers.AddAsync(e, ct);
        return added;
    }
}
