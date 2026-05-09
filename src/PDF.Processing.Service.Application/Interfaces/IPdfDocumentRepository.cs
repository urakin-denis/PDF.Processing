using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Application.Interfaces;

public interface IPdfDocumentRepository
{
    Task AddUploadedAsync(
        Document document,
        DateTimeOffset occurredAtUtc,
        string outboxType,
        string outboxPayloadJson,
        CancellationToken ct);

    Task<IReadOnlyList<Document>> ListDocumentsAsync(int page, int pageSize, CancellationToken ct);

    Task<Document?> GetDocumentAsync(Guid id, CancellationToken ct);

    Task<int> GetAttemptCountAsync(Guid documentId, CancellationToken ct);

    Task<string?> GetLastErrorAsync(Guid documentId, CancellationToken ct);

    Task<string?> GetTextAsync(Guid documentId, CancellationToken ct);
}

