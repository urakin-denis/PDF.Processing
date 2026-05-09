namespace PDF.Processing.Service.Application.Interfaces;

public record OutboxPendingMessage(
    Guid Id,
    DateTimeOffset OccurredAtUtc,
    string Type,
    string PayloadJson,
    int RetryCount);

public interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxPendingMessage>> GetPendingBatchAsync(int take, CancellationToken ct);
    Task MarkPublishedAsync(Guid outboxMessageId, DateTimeOffset publishedAtUtc, CancellationToken ct);
    Task MarkFailedAsync(Guid outboxMessageId, string error, DateTimeOffset nextRetryAtUtc, CancellationToken ct);
    Task TryMarkDocumentQueuedAsync(Guid documentId, DateTimeOffset queuedAtUtc, CancellationToken ct);
}

