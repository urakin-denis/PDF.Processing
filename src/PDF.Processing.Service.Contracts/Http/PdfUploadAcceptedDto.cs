namespace PDF.Processing.Service.Contracts.Http;

public record PdfUploadAcceptedDto(
    Guid DocumentId,
    string Status);

