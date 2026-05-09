using PDF.Processing.Service.Contracts.Messaging;

namespace PDF.Processing.Service.Application.Pdf;

public enum PdfProcessOutcome
{
    NotFound = 0,
    Succeeded = 1,
    Retry = 2,
    DeadLetter = 3
}

public interface IPdfProcessService
{
    Task<PdfProcessOutcome> ProcessAsync(PdfProcessRequested message, CancellationToken ct);
}

