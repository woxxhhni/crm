using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Domain.Extensions;
using Cls.Shared.Exceptions;
using Cls.Shared.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;

namespace Cls.Application.Files;

public record UploadFileCommand(UploadFileBucket Bucket, string OriginalFilename, Stream Content, string MimeType, long Size, int UploadedByUserId) : IRequest<StoredFile>;
public record DeleteFileCommand(int Id) : IRequest<bool>;
public record GetDownloadUrlQuery(int Id, TimeSpan Expiry) : IRequest<string>;
public record GetFileByIdQuery(int Id) : IRequest<StoredFile?>;

public class UploadFileHandler(IUnitOfWork uow, IObjectStorageService storage, IConfiguration cfg) : IRequestHandler<UploadFileCommand, StoredFile>
{
    public async Task<StoredFile> Handle(UploadFileCommand r, CancellationToken ct)
    {
        await storage.EnsureBucketExistsAsync(ct);

        var folderPrefix = r.Bucket.ToFolderPrefix();
        var storedName = $"{Guid.NewGuid():N}_{Path.GetFileName(r.OriginalFilename)}";
        var objectPath = $"{folderPrefix}{storedName}";

        await storage.UploadAsync(objectPath, r.Content, r.MimeType ?? "application/octet-stream", r.Size, ct);

        var f = new StoredFile
        {
            OriginalFilename = r.OriginalFilename,
            StoredFilename = storedName,
            FilePath = objectPath,
            Category = r.Bucket,
            FileSize = (int)r.Size,
            MimeType = r.MimeType ?? "application/octet-stream",
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = r.UploadedByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.StoredFiles.AddAsync(f, ct);
        return f;
    }
}
public class DeleteFileHandler(IUnitOfWork uow, IObjectStorageService storage) : IRequestHandler<DeleteFileCommand, bool>
{
    public async Task<bool> Handle(DeleteFileCommand r, CancellationToken ct)
    {
        var f = await uow.StoredFiles.GetByIdAsync(r.Id, ct);
        if (f is null)
            return false;

        // Remove from MinIO storage first
        await storage.DeleteAsync(f.FilePath, ct);

        // Then remove from database
        await uow.StoredFiles.DeleteAsync(f, ct);
        return true;
    }
}
public class GetDownloadUrlHandler(IUnitOfWork uow, IObjectStorageService storage) : IRequestHandler<GetDownloadUrlQuery, string>
{
    public async Task<string> Handle(GetDownloadUrlQuery r, CancellationToken ct)
    {
        var f = await uow.StoredFiles.Query().FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new InvalidActionException("File not found");

        // FilePath stores the full object path (e.g. "clients/profiles/guid_file.jpg")
        // For legacy files without folder prefix, fall back to StoredFilename
        var objectName = !string.IsNullOrWhiteSpace(f.FilePath) ? f.FilePath : f.StoredFilename;
        return await storage.GetPresignedGetUrlAsync(objectName, r.Expiry, ct);
    }
}
public class GetFileByIdHandler(IUnitOfWork uow) : IRequestHandler<GetFileByIdQuery, StoredFile?>
{
    public Task<StoredFile?> Handle(GetFileByIdQuery r, CancellationToken ct) => uow.StoredFiles.GetByIdAsync(r.Id, ct);
}