using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Infrastructure.Persistence.Configurations;

internal class DocumentTextConfiguration : IEntityTypeConfiguration<DocumentText>
{
    public void Configure(EntityTypeBuilder<DocumentText> builder)
    {
        builder.ToTable("document_text");

        builder.HasKey(x => x.DocumentId);
        builder.Property(x => x.DocumentId).HasColumnName("document_id").ValueGeneratedNever();

        builder.Property(x => x.TextContent).HasColumnName("text_content").IsRequired();
        builder.Property(x => x.ExtractedAtUtc).HasColumnName("extracted_at_utc").IsRequired();
    }
}

