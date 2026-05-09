namespace PDF.Processing.Service.Domain.Documents;

public class DocumentText
{
    public Guid DocumentId { get; private set; }
    public string TextContent { get; private set; } = default!;
    public DateTimeOffset ExtractedAtUtc { get; private set; }

    public Document Document { get; private set; } = default!;

    private DocumentText() { }

    public DocumentText(Guid documentId, string textContent, DateTimeOffset extractedAtUtc)
    {
        if (documentId == Guid.Empty) throw new ArgumentException("Document id must not be empty.", nameof(documentId));
        if (string.IsNullOrWhiteSpace(textContent)) throw new ArgumentException("TextContent is required.", nameof(textContent));

        DocumentId = documentId;
        TextContent = textContent;
        ExtractedAtUtc = extractedAtUtc;
    }

    public void UpdateText(string textContent, DateTimeOffset extractedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(textContent)) throw new ArgumentException("TextContent is required.", nameof(textContent));
        TextContent = textContent;
        ExtractedAtUtc = extractedAtUtc;
    }
}

