using System.ComponentModel.DataAnnotations;

namespace Cls.Infrastructure.Options;

public sealed class ObjectStorageOptions
{
    [Required]
    public string Provider { get; init; } = "minio";

    [Required]
    public string Bucket { get; init; } = string.Empty;
}

public sealed class MinioOptions
{
    [Required] 
    public string Endpoint { get; init; } = string.Empty;
    [Required] 
    public string PublicEndpoint { get; init; } = string.Empty;
    [Required] 
    public string AccessKey { get; init; } = string.Empty;
    [Required] 
    public string SecretKey { get; init; } = string.Empty;

    public bool UseSSL { get; init; } = false;
}
