using Microsoft.EntityFrameworkCore;
using PDF.Processing.Service.Application.Interfaces;
using PDF.Processing.Service.Domain.Documents;
using PDF.Processing.Service.Infrastructure.Persistence.Outbox;

namespace PDF.Processing.Service.Infrastructure.Persistence.Repositories;

public class PdfDocumentRepository : IPdfDocumentRepository
{
    private readonly AppDbContext _db;

    public PdfDocumentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddUploadedAsync(
        Document document,
        DateTimeOffset occurredAtUtc,
        string outboxType,
        string outboxPayloadJson,
        CancellationToken ct)
    {
        _db.Documents.Add(document);
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = occurredAtUtc,
            Type = outboxType,
            PayloadJson = outboxPayloadJson,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> ListDocumentsAsync(int page, int pageSize, CancellationToken ct)
    {
        return await _db.Documents
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Document?> GetDocumentAsync(Guid id, CancellationToken ct)
    {
        return await _db.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<int> GetAttemptCountAsync(Guid documentId, CancellationToken ct)
    {
        return await _db.DocumentProcessingAttempts
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .CountAsync(ct);
    }

    public async Task<string?> GetLastErrorAsync(Guid documentId, CancellationToken ct)
    {
        return await _db.DocumentProcessingAttempts
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId && x.ErrorMessage != null)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => x.ErrorMessage)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<string?> GetTextAsync(Guid documentId, CancellationToken ct)
    {
        return await _db.DocumentTexts
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .Select(x => x.TextContent)
            .FirstOrDefaultAsync(ct);
    }
}

