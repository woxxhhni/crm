using Asp.Versioning;
using AutoMapper;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Api.Models;
using Cls.Api.Services;
using Cls.Application.Orders.Commands;
using Cls.Application.Orders.Commands.ExtraProviders;
using Cls.Application.Orders.Commands.Notes;
using Cls.Application.Orders.Commands.Payments;
using Cls.Application.Orders.Queries;
using Cls.Application.Steps.Queries;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Common;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ClsAuthorize]
public class OrdersController(
    IMediator mediator,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IOrderFileOrchestrator orderFiles) : ClsControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> Get(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderDetailQuery(id, currentUserService.Role, currentUserService.UserId), ct);
        if (result.AccessDenied)
            return Forbid();

        return Ok(result.Response);
    }

    [HttpGet("summary")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<OrderSummaryResponse> Summary(CancellationToken ct) =>
        await mediator.Send(new GetOrderSummaryQuery(), ct);

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderResponse>>> List(
        [FromQuery] int[]? clientIds,
        [FromQuery] int[]? providerIds,
        [FromQuery] string[]? statuses,
        [FromQuery] int[]? stepIds,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] decimal? fromAmount,
        [FromQuery] decimal? toAmount,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
    {
        var employeeId = currentUserService.Role == UserRole.Employee ? currentUserService.UserId : (int?)null;
        var result = await mediator.Send(new ListOrdersQuery(
            clientIds,
            providerIds,
            ParseStatuses(statuses),
            stepIds,
            employeeId,
            fromDate,
            toDate,
            fromAmount,
            toAmount,
            BuildPagedRequest(page, pageSize, sortBy, sortDir)), ct);

        return Ok(result);
    }

    [HttpPost]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromForm] OrderCreateRequest req,
        [FromForm] OrderSellInvoiceUpload? sellFiles,
        [FromForm] OrderBuyInvoiceUpload? buyFiles,
        [FromForm] OrderEmployeesRequest? employees,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        var firstStep = await mediator.Send(new GetFirstStepIdQuery(), ct);
        if (firstStep == 0)
            return BadRequest();

        var created = await mediator.Send(new CreateOrderCommand(
            req.Title, req.OrderDate, req.Description, req.BuyCurrency, req.BuyAmount, req.SellCurrency,
            req.SellAmount, req.ClientId, req.ProviderId, firstStep, userId), ct);

        await orderFiles.ApplyInvoicesOnCreateAsync(created.Id, userId, sellFiles, buyFiles, ct);

        if (employees?.UserIds is { Count: > 0 })
            await mediator.Send(new AssignOrderEmployeesCommand(created.Id, employees.UserIds), ct);

        return CreatedAtAction(nameof(Get), new { id = created.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, mapper.Map<OrderResponse>(created));
    }

    [HttpPut("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)]
    public async Task<ActionResult<OrderResponse>> Update(
        int id,
        [FromForm] OrderUpdateRequest req,
        [FromForm] OrderSellInvoiceUpload? sellFiles,
        [FromForm] OrderBuyInvoiceUpload? buyFiles,
        [FromForm] OrderEmployeesRequest? employees,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        var updated = await mediator.Send(new UpdateOrderCommand(
            id, req.Title, req.OrderDate, req.Description, req.BuyCurrency, req.BuyAmount, req.SellCurrency,
            req.SellAmount, req.ClientId, req.ProviderId, req.RemovedBuyFileIds, req.RemovedSellFileIds,
            employees?.UserIds.ToList()), ct);

        if (updated is null)
            return NotFound();

        await orderFiles.ApplyInvoicesOnUpdateAsync(
            updated.Id, userId, sellFiles, buyFiles, req.RemovedBuyFileIds, req.RemovedSellFileIds, ct);

        return Ok(mapper.Map<OrderResponse>(updated));
    }

    [HttpPatch("{id:int}/cancel")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<int>> CancelOrder(
        int id,
        [FromForm] OrderChangeStatusRequest request,
        [FromForm] UploadMultipleFileModel? fileModel,
        CancellationToken ct) =>
        Ok(await ChangeOrderStatusAsync(id, request, fileModel, UploadFileBucket.OrderChangeState,
            (fileIds, removed) => new CancelOrderCommand(id, request.ActionDate, request.Description, fileIds, removed), ct));

    [HttpPatch("{id:int}/suspend")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<int>> SuspendOrder(
        int id,
        [FromForm] OrderChangeStatusRequest request,
        [FromForm] UploadMultipleFileModel fileModel,
        CancellationToken ct) =>
        Ok(await ChangeOrderStatusAsync(id, request, fileModel, UploadFileBucket.OrderChangeState,
            (fileIds, removed) => new SuspendOrderCommand(id, request.ActionDate, request.Description, fileIds, removed), ct));

    [HttpPatch("{id:int}/unsuspend")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<int>> UnsuspendOrder(
        int id,
        [FromForm] OrderChangeStatusRequest request,
        [FromForm] UploadMultipleFileModel fileModel,
        CancellationToken ct) =>
        Ok(await ChangeOrderStatusAsync(id, request, fileModel, UploadFileBucket.OrderChangeState,
            (fileIds, removed) => new UnSuspendOrderCommand(id, request.ActionDate, request.Description, fileIds, removed), ct));

    [HttpPatch("{id:int}/complete")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<int>> CompleteOrder(
        int id,
        [FromForm] OrderChangeStatusRequest request,
        [FromForm] UploadMultipleFileModel fileModel,
        CancellationToken ct) =>
        Ok(await ChangeOrderStatusAsync(id, request, fileModel, UploadFileBucket.OrderChangeState,
            (fileIds, removed) => new CompleteOrderCommand(id, request.ActionDate, request.Description, fileIds, removed), ct));

    [HttpPatch("set-step")]
    [Consumes("multipart/form-data")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<int>> SetOrderStep(
        [FromForm] SetOrderStepRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.OrderStepChange, ct);
        var order = await mediator.Send(new SetOrderStepCommand(request.OrderId, request.StepId, request.ActionDate, request.Description, fileIds), ct);
        if (order is null)
            return BadRequest(new InvalidActionException("Chenging order step failed"));

        return Ok(order.Id);
    }

    [HttpPatch("set-step-complete")]
    [Consumes("multipart/form-data")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<int>> SetOrderStepCompleted(
        [FromForm] SetOrderStepCompleteRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.OrderStepComplete, ct);
        var order = await mediator.Send(new SetOrderStepCompleteCommand(request.OrderId, request.ActionDate, request.Description, fileIds), ct);
        if (order is null)
            return BadRequest(new InvalidActionException("Set order step completed failed"));

        return Ok(order.Id);
    }

    [HttpPost("{id:int}/employees")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddEmployees(int id, [FromBody] OrderEmployeesRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(User, out _))
            return Forbid();

        await mediator.Send(new AssignOrderEmployeesCommand(id, request.UserIds), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/employees/{userId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveEmployee(int id, int userId, CancellationToken ct)
    {
        if (!TryGetUserId(User, out _))
            return Forbid();

        await mediator.Send(new RemoveOrderEmployeeCommand(id, userId), ct);
        return NoContent();
    }

    [HttpPut("{id:int}/stages/{stageId:int}/assignee")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetStageAssignee(
        int id,
        int stageId,
        [FromBody] SetOrderStageAssigneeRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out _))
            return Forbid();

        await mediator.Send(new SetOrderStageAssigneeCommand(id, stageId, request.UserId), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/unique-numbers")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    public async Task<IActionResult> AddUniqueNumbers(int id, [FromBody] OrderUniqueNumbersRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        var inputs = req.Items.Select(i => new OrderUniqueNumberInput { Label = i.Label, Value = i.Value }).ToList();
        await mediator.Send(new AddOrderUniqueNumbersCommand(id, inputs, userId), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/unique-numbers/{label}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveUniqueNumber(int id, string label, CancellationToken ct)
    {
        await mediator.Send(new RemoveOrderUniqueNumberCommand(id, label), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/client-payments")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> GetClientPayments(int id, CancellationToken ct, bool downloadExcel = false)
    {
        var paymentInfo = await mediator.Send(new GetOrderClientPaymentsInfoQuery(id), ct);
        if (!downloadExcel)
            return Ok(paymentInfo);

        return Ok(new { ContentBase64 = Convert.ToBase64String(paymentInfo.Transactions.ToExcelFile()) });
    }

    [HttpGet("{id:int}/client-payments/{paymentId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderPaymentResponse>> GetClientPayment(int id, int paymentId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderClientPaymentByIdQuery(id, paymentId), ct));

    [HttpPost("{id:int}/client-payments")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddClientPayment(
        int id,
        [FromForm] CreatePaymentRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.ClientPayment, ct);
        await mediator.Send(new AddOrderClientPaymentCommand(id, request.Amount, request.PaymentType, request.Description, fileIds, currentUserService.UserId), ct);
        return NoContent();
    }

    [HttpPut("{id:int}/client-payments/{paymentId}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateClientPayment(
        int id,
        int paymentId,
        [FromForm] UpdatePaymentRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.ClientPayment, ct);
        await mediator.Send(new UpdateOrderClientPaymentCommand(
            id, paymentId, request.Amount, request.PaymentType, request.Description,
            fileIds, request.RemovedFileIds ?? [], currentUserService.UserId), ct);
        await orderFiles.RemoveFilesAsync(request.RemovedFileIds, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/client-payments/{paymentId}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveClientPayment(int id, int paymentId, CancellationToken ct)
    {
        await mediator.Send(new RemoveOrderClientPaymentCommand(id, paymentId, currentUserService.UserId), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/provider-payments")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderPaymentAccountingResponse>> GetProviderPayments(int id, CancellationToken ct, bool downloadExcel = false)
    {
        var paymentInfo = await mediator.Send(new GetOrderProviderPaymentsInfoQuery(id), ct);
        if (!downloadExcel)
            return Ok(paymentInfo);

        return Ok(new { ContentBase64 = Convert.ToBase64String(paymentInfo.Transactions.ToExcelFile()) });
    }

    [HttpGet("{id:int}/provider-payments/{paymentId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderPaymentResponse>> GetProviderPayment(int id, int paymentId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderProviderPaymentByIdQuery(id, paymentId), ct));

    [HttpPost("{id:int}/provider-payments")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddProviderPayment(
        int id,
        [FromForm] CreatePaymentRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.ProviderPayment, ct);
        await mediator.Send(new AddOrderProviderPaymentCommand(id, request.Amount, request.PaymentType, request.Description, fileIds, currentUserService.UserId), ct);
        return NoContent();
    }

    [HttpPut("{id:int}/provider-payments/{paymentId}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProviderPayment(
        int id,
        int paymentId,
        [FromForm] UpdatePaymentRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.ProviderPayment, ct);
        await mediator.Send(new UpdateOrderProviderPaymentCommand(
            id, paymentId, request.Amount, request.PaymentType, request.Description,
            fileIds, request.RemovedFileIds ?? [], currentUserService.UserId), ct);
        await orderFiles.RemoveFilesAsync(request.RemovedFileIds, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/provider-payments/{paymentId}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveProviderPayment(int id, int paymentId, CancellationToken ct)
    {
        await mediator.Send(new RemoveOrderProviderPaymentCommand(id, paymentId, currentUserService.UserId), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/notes")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<OrderNoteListResponse>> GetNotes(int id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderNotesQuery(id), ct));

    [HttpGet("{id:int}/notes/{noteId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<OrderNoteResponse>> GetNote(int id, int noteId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderNoteByIdQuery(id, noteId), ct));

    [HttpPost("{id:int}/notes")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddNote(
        int id,
        [FromForm] CreateNoteRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.OrderNote, ct);
        await mediator.Send(new AddOrderNoteCommand(id, request.ActionDate, request.Title, request.Description, fileIds), ct);
        return NoContent();
    }

    [HttpPut("{id:int}/notes/{noteId}")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateNote(
        int id,
        int noteId,
        [FromForm] UpdateNoteRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.OrderNote, ct);
        await mediator.Send(new UpdateOrderNoteCommand(
            id, noteId, request.ActionDate, request.Title, request.Description,
            fileIds, request.RemovedFileIds ?? []), ct);
        await orderFiles.RemoveFilesAsync(request.RemovedFileIds, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/extra-providers")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddExtraProvider(int id, [FromBody] CreateExtraProviderRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        var extraProviderId = await mediator.Send(new AddOrderExtraProviderCommand(id, request.ProviderId, request.Amount, request.Currency, userId), ct);
        return Ok(extraProviderId);
    }

    [HttpDelete("{id:int}/extra-providers/{epId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveExtraProvider(int id, int epId, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        await mediator.Send(new RemoveOrderExtraProviderCommand(id, epId, userId), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/extra-providers/{epId:int}/payments")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderPaymentAccountingResponse>> GetExtraProviderPayments(int id, int epId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderExtraProviderPaymentsInfoQuery(id, epId), ct));

    [HttpGet("{id:int}/extra-providers/payments/{paymentId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderPaymentResponse>> GetExtraProviderPayment(int id, int paymentId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderExtraProviderPaymentByIdQuery(id, paymentId), ct));

    [HttpPost("{id:int}/extra-providers/{epId:int}/payments")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddExtraProviderPayment(
        int id,
        int epId,
        [FromForm] CreatePaymentRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.ProviderPayment, ct);
        await mediator.Send(new AddOrderExtraProviderPaymentCommand(id, epId, request.Amount, request.PaymentType, request.Description, fileIds, currentUserService.UserId), ct);
        return NoContent();
    }

    [HttpPut("{id:int}/extra-providers/payments/{paymentId}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateExtraProviderPayment(
        int id,
        int paymentId,
        [FromForm] UpdatePaymentRequest request,
        [FromForm] UploadMultipleFileModel? file,
        CancellationToken ct)
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, file?.Files, UploadFileBucket.ProviderPayment, ct);
        await mediator.Send(new UpdateOrderExtraProviderPaymentCommand(
            id, paymentId, request.Amount, request.PaymentType, request.Description,
            fileIds, request.RemovedFileIds ?? [], currentUserService.UserId), ct);
        await orderFiles.RemoveFilesAsync(request.RemovedFileIds, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/extra-providers/payments/{paymentId}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveExtraProviderPayment(int id, int paymentId, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        await mediator.Send(new RemoveOrderExtraProviderPaymentCommand(id, paymentId, userId), ct);
        return NoContent();
    }

    private async Task<int> ChangeOrderStatusAsync<TCommand>(
        int id,
        OrderChangeStatusRequest request,
        UploadMultipleFileModel? fileModel,
        UploadFileBucket bucket,
        Func<List<int>, List<int>, TCommand> createCommand,
        CancellationToken ct) where TCommand : IRequest
    {
        var fileIds = await orderFiles.UploadAsync(currentUserService.UserId, fileModel?.Files, bucket, ct);
        var removed = request.RemovedFileIds ?? [];
        await mediator.Send(createCommand(fileIds, removed), ct);
        await orderFiles.RemoveFilesAsync(removed, ct);
        return id;
    }

    private static OrderStatus[]? ParseStatuses(string[]? statuses)
    {
        if (statuses is not { Length: > 0 })
            return null;

        var parsed = new List<OrderStatus>();
        foreach (var status in statuses)
        {
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var value))
                parsed.Add(value);
        }

        return parsed.Count == 0 ? null : parsed.ToArray();
    }
}
