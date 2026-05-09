using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Infrastructure.Persistence.Configurations;

internal class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FileName).HasColumnName("file_name").IsRequired();
        builder.Property(x => x.Bucket).HasColumnName("bucket").IsRequired();
        builder.Property(x => x.ObjectKey).HasColumnName("object_key").IsRequired();
        builder.HasIndex(x => new { x.Bucket, x.ObjectKey }).IsUnique();

        builder.Property(x => x.SizeBytes).HasColumnName("size_bytes").IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").IsRequired();

        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        builder.HasOne<DocumentStatusLookup>()
            .WithMany()
            .HasForeignKey(x => x.Status)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();

        builder.Property(x => x.ProcessingStartedAtUtc).HasColumnName("processing_started_at_utc");
        builder.Property(x => x.ProcessedAtUtc).HasColumnName("processed_at_utc");

        builder.HasOne(x => x.Text)
            .WithOne(x => x.Document)
            .HasForeignKey<DocumentText>(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

