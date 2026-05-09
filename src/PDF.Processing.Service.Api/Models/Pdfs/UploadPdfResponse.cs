namespace PDF.Processing.Service.Api.Models.Pdfs;

public record UploadPdfResponse(
    Guid DocumentId,
    string Status);

