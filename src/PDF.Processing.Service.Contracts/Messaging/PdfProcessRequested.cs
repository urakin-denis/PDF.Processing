namespace PDF.Processing.Service.Contracts.Messaging;

public record PdfProcessRequested(
    Guid DocumentId,
    string Bucket,
    string ObjectKey,
    string FileName,
    string ContentType,
    DateTimeOffset UploadedAtUtc);

