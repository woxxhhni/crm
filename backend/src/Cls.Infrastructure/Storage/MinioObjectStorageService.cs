using Cls.Application.Abstractions;
using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Configuration;
namespace Cls.Infrastructure.Storage;
public class MinioObjectStorageService : IObjectStorageService
{
    private readonly MinioClient _client;
    private readonly MinioClient _clientPublic;
    private readonly string _bucket;
    public MinioObjectStorageService(IConfiguration cfg)
    {
        _bucket = cfg["ObjectStorage:Bucket"]
                  ?? throw new InvalidOperationException("ObjectStorage:Bucket is not configured.");

        var endpointPublic = cfg["Minio:PublicEndpoint"] ?? cfg["Minio:Endpoint"] ?? "localhost:9000";
        var endpoint = cfg["Minio:Endpoint"] ?? "localhost:9000";
        var access = cfg["Minio:AccessKey"]
                  ?? throw new InvalidOperationException("Minio:AccessKey is not configured.");
        var secret = cfg["Minio:SecretKey"]
                  ?? throw new InvalidOperationException("Minio:SecretKey is not configured.");
        var useSSL = bool.TryParse(cfg["Minio:UseSSL"], out var ssl) && ssl;

        _client = (MinioClient)new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(access, secret)
            .WithSSL(useSSL)
            .Build();

        _clientPublic = (MinioClient)new MinioClient()
            .WithEndpoint(endpointPublic)
            .WithCredentials(access, secret)
            .WithSSL(useSSL)
            .Build();
    }
    public async Task EnsureBucketExistsAsync(CancellationToken ct = default)
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists) 
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
    }
    public async Task UploadAsync(string objectName, Stream data, string contentType, long size, CancellationToken ct = default)
    {
        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(size)
                .WithContentType(contentType),
            ct);
    }
    public async Task<bool> DeleteAsync(string objectName, CancellationToken ct = default)
    {
        await _client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName),
            ct);

        return true;
    }
    public async Task<string> GetPresignedGetUrlAsync(string objectName, TimeSpan expiry, CancellationToken ct = default)
    {
        var seconds = (int)Math.Clamp(expiry.TotalSeconds, 1, int.MaxValue);

        var link = await _clientPublic.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithExpiry(seconds));

        return link;
    }

    public async Task<List<string>> ListObjectsAsync(string? prefix = null, CancellationToken ct = default)
    {
        var objects = new List<string>();
        var args = new ListObjectsArgs().WithBucket(_bucket).WithRecursive(true);
        if (!string.IsNullOrWhiteSpace(prefix))
            args = args.WithPrefix(prefix);

        await foreach (var item in _client.ListObjectsEnumAsync(args, ct))
        {
            if (!item.IsDir)
                objects.Add(item.Key);
        }
        return objects;
    }

    public async Task DownloadAsync(string objectName, Stream destination, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithCallbackStream(async (stream, token) =>
                {
                    try
                    {
                        await stream.CopyToAsync(destination, 81920, token);
                        await destination.FlushAsync(token);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }),
            ct);
        
        await tcs.Task;
    }

    public async Task DeleteByPrefixAsync(string prefix, string? excludePrefix = null, CancellationToken ct = default)
    {
        var objects = await ListObjectsAsync(prefix, ct);
        foreach (var obj in objects)
        {
            if (excludePrefix != null && obj.StartsWith(excludePrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            await _client.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(obj),
                ct);
        }
    }
}
