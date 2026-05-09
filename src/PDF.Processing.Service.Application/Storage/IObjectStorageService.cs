namespace PDF.Processing.Service.Application.Storage;

public interface IObjectStorageService
{
    Task PutObjectAsync(
        string bucket,
        string objectKey,
        Stream content,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> GetObjectAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken);
}

