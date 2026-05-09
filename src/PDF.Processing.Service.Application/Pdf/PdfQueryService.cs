using PDF.Processing.Service.Application.Interfaces;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Application.Pdf;

public class PdfQueryService : IPdfQueryService
{
    private readonly IPdfDocumentRepository _repository;

    public PdfQueryService(IPdfDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PdfListItem>> ListAsync(int page, int pageSize, CancellationToken ct)
    {
        var docs = await _repository.ListDocumentsAsync(page, pageSize, ct);
        return docs
            .Select(x => new PdfListItem(
                x.Id,
                x.FileName,
                (int)x.Status,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToList();
    }

    public async Task<PdfDetails?> GetAsync(Guid id, CancellationToken ct)
    {
        var doc = await _repository.GetDocumentAsync(id, ct);
        if (doc is null) return null;

        var attemptCount = await _repository.GetAttemptCountAsync(id, ct);
        var lastError = await _repository.GetLastErrorAsync(id, ct);

        return new PdfDetails(
            doc.Id,
            doc.FileName,
            doc.Bucket,
            doc.ObjectKey,
            doc.SizeBytes,
            doc.ContentType,
            doc.Status.ToString(),
            doc.CreatedAtUtc,
            doc.UpdatedAtUtc,
            doc.ProcessingStartedAtUtc,
            doc.ProcessedAtUtc,
            lastError,
            attemptCount);
    }

    public async Task<string?> GetTextIfSucceededAsync(Guid id, CancellationToken ct)
    {
        var doc = await _repository.GetDocumentAsync(id, ct);
        if (doc is null) return null;
        if (doc.Status != DocumentStatus.Succeeded) return null;
        return await _repository.GetTextAsync(id, ct);
    }
}

