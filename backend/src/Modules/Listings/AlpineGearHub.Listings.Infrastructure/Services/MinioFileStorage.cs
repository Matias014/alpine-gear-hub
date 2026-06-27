using AlpineGearHub.Listings.Application.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace AlpineGearHub.Listings.Infrastructure.Services;

internal sealed class MinioFileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;

    public MinioFileStorage(IAmazonS3 s3, IConfiguration configuration)
    {
        _s3 = s3;
        _bucket = configuration["Storage:BucketName"] ?? "alpine-gear-hub";
        _publicBaseUrl = configuration["Storage:PublicBaseUrl"] ?? "http://localhost:9000";
    }

    public async Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = storageKey,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true,
        };

        await _s3.PutObjectAsync(request, ct);
        return storageKey;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = storageKey,
        };

        await _s3.DeleteObjectAsync(request, ct);
    }

    public string GetPublicUrl(string storageKey) =>
        $"{_publicBaseUrl.TrimEnd('/')}/{_bucket}/{storageKey}";
}
