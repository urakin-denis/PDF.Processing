namespace PDF.Processing.Service.Api.Models.Pdfs;

public record GetPdfTextResponse(
    Guid DocumentId,
    string Text);

