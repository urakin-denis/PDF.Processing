namespace PDF.Processing.Service.Application.Messaging;

public class OutboxPublishingOptions
{
    public const string SectionName = "OutboxPublishing";

    public int MaxRetries { get; init; } = 20;

    public int BaseRetrySeconds { get; init; } = 5;

    public int MaxRetryDelaySeconds { get; init; } = 900;
}
