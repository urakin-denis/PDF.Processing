namespace PDF.Processing.Service.Contracts.Http;

public record PdfTextDto(
    Guid DocumentId,
    string Text);

