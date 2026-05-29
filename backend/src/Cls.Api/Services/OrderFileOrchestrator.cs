using Cls.Api.Models;
using Cls.Application.Files;
using Cls.Application.Orders.Commands;
using Cls.Domain.Enums;
using MediatR;

namespace Cls.Api.Services;

public interface IOrderFileOrchestrator
{
    Task<List<int>> UploadAsync(int userId, IEnumerable<IFormFile>? files, UploadFileBucket bucket, CancellationToken ct);
    Task RemoveFilesAsync(IEnumerable<int>? fileIds, CancellationToken ct);
    Task ApplyInvoicesOnCreateAsync(
        int orderId,
        int userId,
        OrderSellInvoiceUpload? sellFiles,
        OrderBuyInvoiceUpload? buyFiles,
        CancellationToken ct);
    Task ApplyInvoicesOnUpdateAsync(
        int orderId,
        int userId,
        OrderSellInvoiceUpload? sellFiles,
        OrderBuyInvoiceUpload? buyFiles,
        IEnumerable<int>? removedBuyFileIds,
        IEnumerable<int>? removedSellFileIds,
        CancellationToken ct);
}

public sealed class OrderFileOrchestrator(IMediator mediator, IFileService fileService) : IOrderFileOrchestrator
{
    public Task<List<int>> UploadAsync(int userId, IEnumerable<IFormFile>? files, UploadFileBucket bucket, CancellationToken ct)
    {
        if (files is null)
            return Task.FromResult(new List<int>());

        return fileService.UploadFiles(userId, files, bucket, ct);
    }

    public async Task RemoveFilesAsync(IEnumerable<int>? fileIds, CancellationToken ct)
    {
        foreach (var fileId in fileIds ?? [])
            await mediator.Send(new DeleteFileCommand(fileId), ct);
    }

    public async Task ApplyInvoicesOnCreateAsync(
        int orderId,
        int userId,
        OrderSellInvoiceUpload? sellFiles,
        OrderBuyInvoiceUpload? buyFiles,
        CancellationToken ct)
    {
        var sellFileIds = await UploadAsync(userId, sellFiles?.SFiles, UploadFileBucket.SellInvoice, ct);
        var buyFileIds = await UploadAsync(userId, buyFiles?.BFiles, UploadFileBucket.BuyInvoice, ct);

        if (sellFileIds.Count > 0)
            await mediator.Send(new AddOrderSellInvoicesCommand(orderId, sellFileIds), ct);

        if (buyFileIds.Count > 0)
            await mediator.Send(new AddOrderBuyInvoicesCommand(orderId, buyFileIds), ct);
    }

    public async Task ApplyInvoicesOnUpdateAsync(
        int orderId,
        int userId,
        OrderSellInvoiceUpload? sellFiles,
        OrderBuyInvoiceUpload? buyFiles,
        IEnumerable<int>? removedBuyFileIds,
        IEnumerable<int>? removedSellFileIds,
        CancellationToken ct)
    {
        var sellFileIds = await UploadAsync(userId, sellFiles?.SFiles, UploadFileBucket.SellInvoice, ct);
        var buyFileIds = await UploadAsync(userId, buyFiles?.BFiles, UploadFileBucket.BuyInvoice, ct);

        if (sellFileIds.Count > 0)
            await mediator.Send(new AddOrderSellInvoicesCommand(orderId, sellFileIds), ct);

        if (buyFileIds.Count > 0)
            await mediator.Send(new AddOrderBuyInvoicesCommand(orderId, buyFileIds), ct);

        await RemoveFilesAsync(removedBuyFileIds, ct);
        await RemoveFilesAsync(removedSellFileIds, ct);
    }
}
