using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Cls.Infrastructure.Options;

public sealed class ValidateObjectStorageProviderOptions : IValidateOptions<ObjectStorageOptions>
{
    private readonly IConfiguration _cfg;

    public ValidateObjectStorageProviderOptions(IConfiguration cfg) => _cfg = cfg;

    public ValidateOptionsResult Validate(string? name, ObjectStorageOptions options)
    {
        var provider = (options.Provider ?? "minio").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(options.Bucket))
            return ValidateOptionsResult.Fail("ObjectStorage:Bucket is required.");

        if (provider is "minio")
        {
            // require MinIO settings
            string[] required =
            {
                "Minio:Endpoint", "Minio:PublicEndpoint",
                "Minio:AccessKey", "Minio:SecretKey"
            };

            var missing = required.Where(k => string.IsNullOrWhiteSpace(_cfg[k])).ToArray();
            return missing.Length > 0
                ? ValidateOptionsResult.Fail("Missing MinIO config: " + string.Join(", ", missing))
                : ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail($"Unsupported ObjectStorage provider: {options.Provider}. Use 'minio'.");
    }
}