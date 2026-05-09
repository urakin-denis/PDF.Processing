using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Application.Pdf;

public record UploadPdfResult(
    Guid DocumentId,
    DocumentStatus Status);

public interface IPdfUploadService
{
    Task<UploadPdfResult> UploadAsync(UploadPdfInput input, DateTimeOffset nowUtc, CancellationToken ct);
}

