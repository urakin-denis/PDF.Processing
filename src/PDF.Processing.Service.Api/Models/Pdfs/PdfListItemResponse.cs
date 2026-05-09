namespace PDF.Processing.Service.Api.Models.Pdfs;

public record PdfListItemResponse(
    Guid Id,
    string FileName,
    int Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

