using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Services;

internal static class OrderLoader
{
    internal static async Task<Order> GetWithEmployeesAsync(IUnitOfWork uow, int orderId, CancellationToken ct)
    {
        var order = await uow.Orders.Query()
            .Include(x => x.Employees.Where(e => !e.IsDeleted))
            .Include(x => x.StageAssignments.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == orderId, ct);

        if (order is null)
            throw new NotFoundException();

        return order;
    }
}
