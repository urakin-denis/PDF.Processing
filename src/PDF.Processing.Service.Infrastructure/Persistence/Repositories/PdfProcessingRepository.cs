using Microsoft.EntityFrameworkCore;
using PDF.Processing.Service.Application.Interfaces;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Infrastructure.Persistence.Repositories;

internal class PdfProcessingRepository : IPdfProcessingRepository
{
    private readonly AppDbContext _db;

    public PdfProcessingRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Document?> GetForUpdateAsync(Guid documentId, CancellationToken ct)
    {
        return _db.Documents
            .Include(x => x.Text)
            .FirstOrDefaultAsync(x => x.Id == documentId, ct);
    }

    public Task<int> GetAttemptCountAsync(Guid documentId, CancellationToken ct)
    {
        return _db.DocumentProcessingAttempts
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .CountAsync(ct);
    }

    public Task AddAttemptAsync(DocumentProcessingAttempt attempt, CancellationToken ct)
    {
        _db.DocumentProcessingAttempts.Add(attempt);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}

