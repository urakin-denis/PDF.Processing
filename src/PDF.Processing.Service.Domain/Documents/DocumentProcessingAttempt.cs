namespace PDF.Processing.Service.Domain.Documents;

public class DocumentProcessingAttempt
{
    public long Id { get; private set; }

    public Guid DocumentId { get; private set; }
    public int AttemptNo { get; private set; }
    public DocumentStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public Document Document { get; private set; } = default!;

    private DocumentProcessingAttempt() { }

    public DocumentProcessingAttempt(
        Guid documentId,
        int attemptNo,
        DocumentStatus status,
        string? errorMessage,
        DateTimeOffset occurredAtUtc)
    {
        if (documentId == Guid.Empty) throw new ArgumentException("Document id must not be empty.", nameof(documentId));
        if (attemptNo <= 0) throw new ArgumentOutOfRangeException(nameof(attemptNo));

        DocumentId = documentId;
        AttemptNo = attemptNo;
        Status = status;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage;
        OccurredAtUtc = occurredAtUtc;
    }
}

