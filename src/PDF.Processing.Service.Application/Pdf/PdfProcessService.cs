using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PDF.Processing.Service.Application.Interfaces;
using PDF.Processing.Service.Application.Storage;
using PDF.Processing.Service.Contracts.Messaging;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Application.Pdf;

public class PdfProcessService : IPdfProcessService
{
    private readonly IPdfProcessingRepository _repository;
    private readonly IObjectStorageService _storage;
    private readonly IPdfTextExtractor _extractor;
    private readonly PdfProcessingOptions _options;
    private readonly ILogger<PdfProcessService> _logger;

    public PdfProcessService(
        IPdfProcessingRepository repository,
        IObjectStorageService storage,
        IPdfTextExtractor extractor,
        IOptions<PdfProcessingOptions> options,
        ILogger<PdfProcessService> logger)
    {
        _repository = repository;
        _storage = storage;
        _extractor = extractor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PdfProcessOutcome> ProcessAsync(PdfProcessRequested message, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var document = await _repository.GetForUpdateAsync(message.DocumentId, ct);
        if (document is null) return PdfProcessOutcome.NotFound;

        document.MarkProcessing(now);
        await _repository.SaveChangesAsync(ct);

        try
        {
            _logger.LogDebug(
                "Fetching PDF from object storage DocumentId={DocumentId} Bucket={Bucket} ObjectKey={ObjectKey}",
                message.DocumentId,
                message.Bucket,
                message.ObjectKey);
            await using var pdfStream = await _storage.GetObjectAsync(message.Bucket, message.ObjectKey, ct);
            var text = await _extractor.ExtractTextAsync(pdfStream, ct);

            document.MarkSucceeded(text, DateTimeOffset.UtcNow);
            await _repository.SaveChangesAsync(ct);
            return PdfProcessOutcome.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "PDF processing step failed DocumentId={DocumentId} Bucket={Bucket} ObjectKey={ObjectKey}. " +
                "If the message mentions unreachable endpoints, this is typically MinIO connectivity (not HTTP API).",
                message.DocumentId,
                message.Bucket,
                message.ObjectKey);
            var attemptNow = DateTimeOffset.UtcNow;
            var attemptsSoFar = await _repository.GetAttemptCountAsync(document.Id, ct);
            var nextAttemptNo = attemptsSoFar + 1;

            var deadLetter = nextAttemptNo >= _options.MaxAttempts;
            var status = deadLetter ? DocumentStatus.DeadLettered : DocumentStatus.Failed;

            await _repository.AddAttemptAsync(new DocumentProcessingAttempt(
                document.Id,
                nextAttemptNo,
                status,
                ex.Message,
                attemptNow), ct);

            if (status == DocumentStatus.DeadLettered) document.MarkDeadLettered(attemptNow);
            else document.MarkFailed(attemptNow);

            await _repository.SaveChangesAsync(ct);

            return deadLetter ? PdfProcessOutcome.DeadLetter : PdfProcessOutcome.Retry;
        }
    }
}

