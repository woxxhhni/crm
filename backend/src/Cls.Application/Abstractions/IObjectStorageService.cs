namespace Cls.Application.Abstractions;

public interface IObjectStorageService
{
    Task UploadAsync(string objectName, Stream data, string contentType, long size, CancellationToken ct = default);
    Task<bool> DeleteAsync(string objectName, CancellationToken ct = default);
    Task<string> GetPresignedGetUrlAsync(string objectName, TimeSpan expiry, CancellationToken ct = default);
    Task EnsureBucketExistsAsync(CancellationToken ct = default);

    /// <summary>Lists all object names under a given prefix.</summary>
    Task<List<string>> ListObjectsAsync(string? prefix = null, CancellationToken ct = default);

    /// <summary>Downloads an object to a stream.</summary>
    Task DownloadAsync(string objectName, Stream destination, CancellationToken ct = default);

    /// <summary>Deletes all objects under a prefix, optionally excluding a specific prefix.</summary>
    Task DeleteByPrefixAsync(string prefix, string? excludePrefix = null, CancellationToken ct = default);
}
