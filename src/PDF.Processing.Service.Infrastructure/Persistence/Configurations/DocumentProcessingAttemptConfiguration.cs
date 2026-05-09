using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Infrastructure.Persistence.Configurations;

internal class DocumentProcessingAttemptConfiguration : IEntityTypeConfiguration<DocumentProcessingAttempt>
{
    public void Configure(EntityTypeBuilder<DocumentProcessingAttempt> builder)
    {
        builder.ToTable("document_processing_attempts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DocumentId).HasColumnName("document_id").IsRequired();
        builder.Property(x => x.AttemptNo).HasColumnName("attempt_no").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message");
        builder.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();

        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => new { x.Status, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.DocumentId, x.AttemptNo }).IsUnique();

        builder.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DocumentStatusLookup>()
            .WithMany()
            .HasForeignKey(x => x.Status)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

