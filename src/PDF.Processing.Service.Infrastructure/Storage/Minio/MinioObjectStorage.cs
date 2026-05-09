using Minio;
using Minio.DataModel.Args;
using PDF.Processing.Service.Application.Storage;

namespace PDF.Processing.Service.Infrastructure.Storage.Minio;

internal class MinioObjectStorage : IObjectStorageService
{
    private readonly IMinioClient _client;

    public MinioObjectStorage(IMinioClient client)
    {
        _client = client;
    }

    public async Task PutObjectAsync(
        string bucket,
        string objectKey,
        Stream content,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken)
    {
        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(sizeBytes)
            .WithContentType(contentType);

        try
        {
            await _client.PutObjectAsync(args, cancellationToken);
        }
        catch (Exception ex)
        {
            throw WrapStorageConnectivity(ex, nameof(PutObjectAsync), bucket, objectKey);
        }
    }

    public async Task<Stream> GetObjectAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(ms));

        try
        {
            await _client.GetObjectAsync(args, cancellationToken);
        }
        catch (Exception ex)
        {
            throw WrapStorageConnectivity(ex, nameof(GetObjectAsync), bucket, objectKey);
        }

        ms.Position = 0;
        return ms;
    }

    private static InvalidOperationException WrapStorageConnectivity(
        Exception inner,
        string operation,
        string bucket,
        string objectKey)
    {
        var msg =
            $"{operation} failed for object storage (MinIO). Bucket={bucket}, ObjectKey={objectKey}. " +
            "Ensure MinIO is running and Minio:Endpoint is reachable from this process (Docker: use service hostname, not localhost). " +
            $"Original error: {inner.Message}";
        return new InvalidOperationException(msg, inner);
    }
}

