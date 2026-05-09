namespace PDF.Processing.Service.Domain.Documents;

public class Document
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string FileName { get; private set; } = default!;
    public string Bucket { get; private set; } = default!;
    public string ObjectKey { get; private set; } = default!;

    public long SizeBytes { get; private set; }
    public string ContentType { get; private set; } = default!;

    public DocumentStatus Status { get; private set; } = DocumentStatus.Uploaded;

    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessingStartedAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public DocumentText? Text { get; private set; }

    private Document() { }

    public Document(
        Guid id,
        string fileName,
        string bucket,
        string objectKey,
        long sizeBytes,
        string contentType,
        DateTimeOffset nowUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("Document id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("FileName is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(bucket)) throw new ArgumentException("Bucket is required.", nameof(bucket));
        if (string.IsNullOrWhiteSpace(objectKey)) throw new ArgumentException("ObjectKey is required.", nameof(objectKey));
        if (sizeBytes < 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("ContentType is required.", nameof(contentType));

        Id = id;
        FileName = fileName;
        Bucket = bucket;
        ObjectKey = objectKey;
        SizeBytes = sizeBytes;
        ContentType = contentType;
        Status = DocumentStatus.Uploaded;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkQueued(DateTimeOffset nowUtc)
    {
        Status = DocumentStatus.Queued;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkProcessing(DateTimeOffset nowUtc)
    {
        Status = DocumentStatus.Processing;
        ProcessingStartedAtUtc ??= nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkSucceeded(string textContent, DateTimeOffset nowUtc)
    {
        Status = DocumentStatus.Succeeded;
        if (Text is null) Text = new DocumentText(Id, textContent, nowUtc);
        else Text.UpdateText(textContent, nowUtc);
        ProcessedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkFailed(DateTimeOffset nowUtc)
    {
        Status = DocumentStatus.Failed;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkDeadLettered(DateTimeOffset nowUtc)
    {
        Status = DocumentStatus.DeadLettered;
        UpdatedAtUtc = nowUtc;
    }
}

