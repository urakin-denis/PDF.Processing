namespace PDF.Processing.Service.Contracts.Http;

public record PdfDetailsDto(
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

