namespace PDF.Processing.Service.Infrastructure.Persistence.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }

    public string Type { get; set; } = default!;
    public string PayloadJson { get; set; } = default!;

    public OutboxMessageStatus Status { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; set; }
    public string? LastError { get; set; }
    public int RetryCount { get; set; }

    public DateTimeOffset? NextRetryAtUtc { get; set; }
}

