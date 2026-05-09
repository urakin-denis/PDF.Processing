using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Application.Interfaces;

public interface IPdfProcessingRepository
{
    Task<Document?> GetForUpdateAsync(Guid documentId, CancellationToken ct);
    Task<int> GetAttemptCountAsync(Guid documentId, CancellationToken ct);
    Task AddAttemptAsync(DocumentProcessingAttempt attempt, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

