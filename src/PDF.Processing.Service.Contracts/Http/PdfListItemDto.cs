namespace PDF.Processing.Service.Contracts.Http;

public record PdfListItemDto(
    Guid Id,
    string FileName,
    int Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

