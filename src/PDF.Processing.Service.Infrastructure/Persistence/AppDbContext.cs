using Microsoft.EntityFrameworkCore;
using PDF.Processing.Service.Domain.Documents;
using PDF.Processing.Service.Infrastructure.Persistence.Outbox;

namespace PDF.Processing.Service.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentText> DocumentTexts => Set<DocumentText>();
    public DbSet<DocumentProcessingAttempt> DocumentProcessingAttempts => Set<DocumentProcessingAttempt>();
    public DbSet<DocumentStatusLookup> DocumentStatuses => Set<DocumentStatusLookup>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

