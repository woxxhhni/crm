using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Files;
using Cls.Application.Orders.Services;
using Cls.Application.Steps.Queries;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderDetailQuery(int Id, UserRole Role, int UserId) : IRequest<OrderDetailResult>;

public record OrderDetailResult(OrderResponse? Response, bool AccessDenied);

public class GetOrderDetailQueryHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator)
    : IRequestHandler<GetOrderDetailQuery, OrderDetailResult>
{
    private static readonly TimeSpan UrlLifetime = TimeSpan.FromHours(1);

    public async Task<OrderDetailResult> Handle(GetOrderDetailQuery request, CancellationToken ct)
    {
        var order = await LoadOrderAsync(request.Id, ct);
        if (order is null)
            throw new NotFoundException();

        if (request.Role == UserRole.Employee &&
            !order.Employees.Any(p => p.UserId == request.UserId) &&
            !order.StageAssignments.Any(p => p.UserId == request.UserId))
        {
            return new OrderDetailResult(null, AccessDenied: true);
        }

        var response = mapper.Map<OrderResponse>(order);
        var histories = await mediator.Send(new GetOrderStepHistoryByOrderIdQuery(order.Id), ct);
        await PopulateStepLogsAsync(order, response, histories, ct);
        await PopulateStageAssignmentsAsync(order, response, ct);
        await PopulateInvoiceLinksAsync(order, response, ct);
        await PopulateEmployeeProfileUrlsAsync(response, ct);

        return new OrderDetailResult(response, AccessDenied: false);
    }

    private async Task PopulateStepLogsAsync(
        Order order,
        OrderResponse response,
        IReadOnlyList<OrderStepHistory> histories,
        CancellationToken ct)
    {
        var workflowSteps = await uow.Steps.Query()
            .Include(s => s.Stage)
            .Where(s => s.IsActive && s.Stage.IsActive)
            .OrderBy(s => s.OrderPosition)
            .AsNoTracking()
            .ToListAsync(ct);

        var visibleLogs = order.Logs
            .Where(l => OrderLogVisibility.IsVisible(l.LogType))
            .OrderBy(l => l.CreatedAt)
            .ToList();

        var stepIdsInVisitOrder = workflowSteps.Select(s => s.Id).ToList();
        foreach (var history in histories.OrderBy(h => h.EnteredAt))
        {
            if (!stepIdsInVisitOrder.Contains(history.StepId))
                stepIdsInVisitOrder.Add(history.StepId);
        }

        // Include steps that have visible logs but no history row (defensive)
        foreach (var log in visibleLogs.Where(l => l.StepId.HasValue))
        {
            var stepId = log.StepId!.Value;
            if (!stepIdsInVisitOrder.Contains(stepId))
                stepIdsInVisitOrder.Add(stepId);
        }

        foreach (var stepId in stepIdsInVisitOrder)
        {
            var stepHistories = histories.Where(h => h.StepId == stepId).OrderBy(h => h.EnteredAt).ToList();
            var firstHistory = stepHistories.FirstOrDefault();
            var lastHistory = stepHistories.LastOrDefault();

            var stepMeta = workflowSteps.FirstOrDefault(s => s.Id == stepId)
                ?? firstHistory?.Step
                ?? order.Logs.FirstOrDefault(l => l.StepId == stepId)?.Step;

            if (stepMeta is null)
                continue;

            var stepGroup = new OrderStepLogsResponse
            {
                StepId = stepId,
                StepName = stepMeta.Name,
                StageId = stepMeta.StageId,
                StageName = stepMeta.Stage?.Name ?? string.Empty,
                StepOrderPosition = stepMeta.OrderPosition,
                StepEnteredAt = firstHistory?.EnteredAt,
                StepExitedAt = lastHistory?.ExitedAt,
                IsCompleted = lastHistory?.ExitedAt.HasValue == true,
                IsCurrent = order.CurrentStepId == stepId
                    && lastHistory?.ExitedAt.HasValue != true
                    && (order.Status == OrderStatus.InProgress || order.Status == OrderStatus.Suspended)
            };

            foreach (var log in visibleLogs.Where(l => l.StepId == stepId))
            {
                var logResponse = mapper.Map<OrderLogResponse>(log);
                await EnrichLogResponseAsync(logResponse, histories, log, ct);
                stepGroup.Logs.Add(logResponse);
            }

            response.Steps.Add(stepGroup);
        }
    }

    private async Task EnrichLogResponseAsync(
        OrderLogResponse logResponse,
        IReadOnlyList<OrderStepHistory> histories,
        OrderLog log,
        CancellationToken ct)
    {
        if (logResponse.ActorProfileFileId.HasValue)
        {
            logResponse.ActorProfileUrl = await mediator.Send(
                new GetDownloadUrlQuery(logResponse.ActorProfileFileId.Value, UrlLifetime), ct);
        }

        foreach (var file in logResponse.Files)
            file.Url = await mediator.Send(new GetDownloadUrlQuery(file.Id, UrlLifetime), ct);

        logResponse.StepExitedAt = histories
            .Where(h => h.StepId == log.StepId && h.CreatedAt < log.CreatedAt)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => h.ExitedAt)
            .FirstOrDefault();
    }

    private async Task PopulateInvoiceLinksAsync(Order order, OrderResponse response, CancellationToken ct)
    {
        foreach (var invoice in order.SellInvoices)
        {
            var link = await mediator.Send(new GetDownloadUrlQuery(invoice.FileId, UrlLifetime), ct);
            response.SellInvoiceLinks.Add(new OrderInvoiceFileResponse
            {
                Id = invoice.Id,
                OrderId = invoice.OrderId,
                FileId = invoice.FileId,
                FileName = invoice.File.OriginalFilename,
                Url = link
            });
        }

        foreach (var invoice in order.BuyInvoices)
        {
            var link = await mediator.Send(new GetDownloadUrlQuery(invoice.FileId, UrlLifetime), ct);
            response.BuyInvoiceLinks.Add(new OrderInvoiceFileResponse
            {
                Id = invoice.Id,
                OrderId = invoice.OrderId,
                FileId = invoice.FileId,
                FileName = invoice.File.OriginalFilename,
                Url = link
            });
        }
    }

    private async Task PopulateStageAssignmentsAsync(Order order, OrderResponse response, CancellationToken ct)
    {
        var stages = await uow.Stages.Query()
            .Where(s => s.IsActive)
            .OrderBy(s => s.OrderPosition)
            .AsNoTracking()
            .ToListAsync(ct);

        var assignments = await uow.OrderStageAssignments.Query()
            .Include(x => x.User)
            .Where(x => x.OrderId == order.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        var firstOrderEmployee = order.Employees
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefault();
        var firstStageId = stages.FirstOrDefault()?.Id;

        foreach (var stage in stages)
        {
            var assignment = assignments.FirstOrDefault(x => x.StageId == stage.Id);
            var fallbackEmployee = assignment is null && stage.Id == firstStageId ? firstOrderEmployee : null;
            var user = assignment?.User ?? fallbackEmployee?.User;
            var profileFileId = user?.FileId;

            var responseItem = new OrderStageAssigneeResponse
            {
                StageId = stage.Id,
                StageName = stage.Name,
                StageOrderPosition = stage.OrderPosition,
                EmployeeId = user?.Id,
                EmployeeName = user?.Name,
                EmployeeEmail = user?.Email,
                ProfileFileId = profileFileId,
                AssignedAt = assignment?.CreatedAt ?? fallbackEmployee?.CreatedAt,
                AssignedByUserId = assignment?.CreatedByUserId ?? fallbackEmployee?.CreatedByUserId
            };

            if (profileFileId.HasValue)
                responseItem.ProfileUrl = await mediator.Send(new GetDownloadUrlQuery(profileFileId.Value, UrlLifetime), ct);

            response.StageAssignments.Add(responseItem);
        }
    }

    private async Task PopulateEmployeeProfileUrlsAsync(OrderResponse response, CancellationToken ct)
    {
        foreach (var employee in response.Employees.Where(x => x.ProfileFileId.HasValue))
        {
            employee.ProfileUrl = await mediator.Send(
                new GetDownloadUrlQuery(employee.ProfileFileId!.Value, UrlLifetime), ct);
        }
    }

    private async Task<Order?> LoadOrderAsync(int id, CancellationToken ct)
    {
        var order = await uow.Orders.Query()
            .Include(p => p.Client)
            .Include(p => p.Provider)
            .Include(p => p.ExtraProviders).ThenInclude(p => p.Provider)
            .Include(p => p.ExtraProviders).ThenInclude(p => p.Payments)
            .Include(p => p.SellInvoices).ThenInclude(p => p.File)
            .Include(p => p.BuyInvoices).ThenInclude(p => p.File)
            .Include(p => p.UniqueNumbers)
            .Include(p => p.CurrentStep).ThenInclude(p => p.Stage)
            .Include(p => p.Employees.Where(e => !e.IsDeleted)).ThenInclude(p => p.User)
            .Include(p => p.StageAssignments.Where(e => !e.IsDeleted)).ThenInclude(p => p.User)
            .Include(p => p.Logs).ThenInclude(p => p.Step).ThenInclude(p => p.Stage)
            .Include(p => p.Logs).ThenInclude(p => p.Files).ThenInclude(p => p.File)
            .Include(x => x.Logs).ThenInclude(x => x.ActorUser)
            .Include(x => x.Logs).ThenInclude(x => x.FromStep)
            .Include(x => x.Logs).ThenInclude(x => x.ToStep)
            .Include(x => x.Notes).ThenInclude(x => x.Files).ThenInclude(x => x.File)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (order is null)
            return null;

        return order;
    }
}
