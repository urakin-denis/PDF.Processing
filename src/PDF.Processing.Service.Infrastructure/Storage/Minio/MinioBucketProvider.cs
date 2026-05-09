using Microsoft.Extensions.Options;
using PDF.Processing.Service.Application.Storage;

namespace PDF.Processing.Service.Infrastructure.Storage.Minio;

internal class MinioBucketProvider : IObjectStorageBucketProvider
{
    private readonly MinioOptions _options;

    public MinioBucketProvider(IOptions<MinioOptions> options)
    {
        _options = options.Value;
    }

    public string GetBucket() => _options.Bucket;
}

