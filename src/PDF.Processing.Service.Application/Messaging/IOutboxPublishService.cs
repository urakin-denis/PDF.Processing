namespace PDF.Processing.Service.Application.Messaging;

public interface IOutboxPublishService
{
    Task PublishPendingAsync(CancellationToken ct);
}

