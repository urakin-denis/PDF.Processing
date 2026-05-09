namespace PDF.Processing.Service.Application.Pdf;

public record UploadPdfInput(
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content);

