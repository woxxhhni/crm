using MediatR;
using Cls.Application.Abstractions;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Users.Commands;
public record DeleteUserCommand(int Id) : IRequest<bool>;
public class DeleteUserCommandHandler(IUnitOfWork uow, ICurrentUserService currentUserService) : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await uow.Users.GetByIdAsync(request.Id, ct);
        var assignedOrders = await uow.Orders.Query()
            .Include(x=>x.Employees)
            .Include(x=>x.StageAssignments)
            .Where(x => x.Employees.Any(c => c.UserId == user.Id) || x.StageAssignments.Any(c => c.UserId == user.Id)).ToListAsync(ct);
        foreach (var order in assignedOrders)
        {
           foreach (var employee in order.Employees.Where(x => x.UserId == user.Id))
               employee.Remove(currentUserService.UserId);

           foreach (var assignment in order.StageAssignments.Where(x => x.UserId == user.Id))
           {
               assignment.IsDeleted = true;
               assignment.DeletedAt = DateTime.UtcNow;
               assignment.DeletedByUserId = currentUserService.UserId;
           }
           //TODO: BatchUpdate
           await uow.Orders.UpdateAsync(order, ct);

        }
        await uow.Users.DeleteAsync(user, ct);
        return true;
    }
}
