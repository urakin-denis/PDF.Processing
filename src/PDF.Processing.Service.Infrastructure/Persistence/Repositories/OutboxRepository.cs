using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PDF.Processing.Service.Application.Interfaces;
using PDF.Processing.Service.Application.Messaging;
using PDF.Processing.Service.Domain.Documents;
using PDF.Processing.Service.Infrastructure.Persistence.Outbox;

namespace PDF.Processing.Service.Infrastructure.Persistence.Repositories;

internal class OutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _db;
    private readonly OutboxPublishingOptions _options;

    public OutboxRepository(AppDbContext db, IOptions<OutboxPublishingOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<OutboxPendingMessage>> GetPendingBatchAsync(int take, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        return await _db.OutboxMessages
            .AsNoTracking()
            .Where(x =>
                x.Status == OutboxMessageStatus.Pending
                || (
                    x.Status == OutboxMessageStatus.Failed
                    && x.RetryCount < _options.MaxRetries
                    && (x.NextRetryAtUtc == null || x.NextRetryAtUtc <= now)))
            .OrderBy(x => x.OccurredAtUtc)
            .Take(take)
            .Select(x => new OutboxPendingMessage(x.Id, x.OccurredAtUtc, x.Type, x.PayloadJson, x.RetryCount))
            .ToListAsync(ct);
    }

    public async Task MarkPublishedAsync(Guid outboxMessageId, DateTimeOffset publishedAtUtc, CancellationToken ct)
    {
        var msg = await _db.OutboxMessages.FirstOrDefaultAsync(x => x.Id == outboxMessageId, ct);
        if (msg is null) return;

        msg.Status = OutboxMessageStatus.Published;
        msg.PublishedAtUtc = publishedAtUtc;
        msg.LastError = null;
        msg.NextRetryAtUtc = null;

        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Guid outboxMessageId, string error, DateTimeOffset nextRetryAtUtc, CancellationToken ct)
    {
        var msg = await _db.OutboxMessages.FirstOrDefaultAsync(x => x.Id == outboxMessageId, ct);
        if (msg is null) return;

        msg.Status = OutboxMessageStatus.Failed;
        msg.RetryCount++;
        msg.LastError = error;
        msg.NextRetryAtUtc = nextRetryAtUtc;

        await _db.SaveChangesAsync(ct);
    }

    public async Task TryMarkDocumentQueuedAsync(Guid documentId, DateTimeOffset queuedAtUtc, CancellationToken ct)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(x => x.Id == documentId, ct);
        if (doc is null) return;
        if (doc.Status == DocumentStatus.Uploaded)
        {
            doc.MarkQueued(queuedAtUtc);
            await _db.SaveChangesAsync(ct);
        }
    }
}
