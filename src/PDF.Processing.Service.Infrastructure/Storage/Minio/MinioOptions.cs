namespace PDF.Processing.Service.Infrastructure.Storage.Minio;

public class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; init; } = default!;
    public string AccessKey { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public bool UseSsl { get; init; }
    public string Bucket { get; init; } = "pdfs";
}

