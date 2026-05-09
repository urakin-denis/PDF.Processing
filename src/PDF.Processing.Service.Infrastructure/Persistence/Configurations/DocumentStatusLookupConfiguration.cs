using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Infrastructure.Persistence.Configurations;

internal class DocumentStatusLookupConfiguration : IEntityTypeConfiguration<DocumentStatusLookup>
{
    public void Configure(EntityTypeBuilder<DocumentStatusLookup> builder)
    {
        builder.ToTable("document_statuses");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion<int>().ValueGeneratedNever();

        builder.Property(x => x.Name).HasColumnName("name").IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(
            new DocumentStatusLookup(DocumentStatus.Uploaded, nameof(DocumentStatus.Uploaded)),
            new DocumentStatusLookup(DocumentStatus.Queued, nameof(DocumentStatus.Queued)),
            new DocumentStatusLookup(DocumentStatus.Processing, nameof(DocumentStatus.Processing)),
            new DocumentStatusLookup(DocumentStatus.Succeeded, nameof(DocumentStatus.Succeeded)),
            new DocumentStatusLookup(DocumentStatus.Failed, nameof(DocumentStatus.Failed)),
            new DocumentStatusLookup(DocumentStatus.DeadLettered, nameof(DocumentStatus.DeadLettered)));
    }
}

