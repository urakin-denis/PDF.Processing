namespace PDF.Processing.Service.Application.Pdf;

public record PdfListItem(
    Guid Id,
    string FileName,
    int Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public record PdfDetails(
    Guid Id,
    string FileName,
    string Bucket,
    string ObjectKey,
    long SizeBytes,
    string ContentType,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ProcessingStartedAtUtc,
    DateTimeOffset? ProcessedAtUtc,
    string? LastError,
    int AttemptCount);

public interface IPdfQueryService
{
    Task<IReadOnlyList<PdfListItem>> ListAsync(int page, int pageSize, CancellationToken ct);
    Task<PdfDetails?> GetAsync(Guid id, CancellationToken ct);
    Task<string?> GetTextIfSucceededAsync(Guid id, CancellationToken ct);
}

