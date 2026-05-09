using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PDF.Processing.Service.Application.Interfaces;

namespace PDF.Processing.Service.Application.Messaging;

public class OutboxPublishService : IOutboxPublishService
{
    private readonly IOutboxRepository _outbox;
    private readonly IMessagePublisher _publisher;
    private readonly IMessageRoutingProvider _routingProvider;
    private readonly OutboxPublishingOptions _options;
    private readonly ILogger<OutboxPublishService> _logger;

    public OutboxPublishService(
        IOutboxRepository outbox,
        IMessagePublisher publisher,
        IMessageRoutingProvider routingProvider,
        IOptions<OutboxPublishingOptions> options,
        ILogger<OutboxPublishService> logger)
    {
        _outbox = outbox;
        _publisher = publisher;
        _routingProvider = routingProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishPendingAsync(CancellationToken ct)
    {
        var (exchange, routingKey) = _routingProvider.GetOutboxRouting();

        var batch = await _outbox.GetPendingBatchAsync(take: 50, ct);
        if (batch.Count == 0) return;

        foreach (var msg in batch)
        {
            try
            {
                using var doc = JsonDocument.Parse(msg.PayloadJson, new JsonDocumentOptions { AllowTrailingCommas = true });
                var root = doc.RootElement.Clone();

                await _publisher.PublishAsync(exchange, routingKey, root, ct);
                await _outbox.MarkPublishedAsync(msg.Id, DateTimeOffset.UtcNow, ct);

                if (TryGetDocumentId(root, out var documentId))
                {
                    await _outbox.TryMarkDocumentQueuedAsync(documentId, DateTimeOffset.UtcNow, ct);
                }
            }
            catch (Exception ex)
            {
                var now = DateTimeOffset.UtcNow;
                var delay = CalculateBackoff(msg.RetryCount);
                var nextRetryAt = now.Add(delay);
                var text = $"RabbitMQ publish: {ex.Message}";
                await _outbox.MarkFailedAsync(msg.Id, text, nextRetryAt, ct);
                _logger.LogWarning(
                    ex,
                    "Outbox publish failed OutboxId={OutboxId} RetryCountBefore={RetryBefore} NextRetryAtUtc={NextRetry}. Check RabbitMq:Host/port from this process (not HTTP API).",
                    msg.Id,
                    msg.RetryCount,
                    nextRetryAt);
            }
        }
    }

    private TimeSpan CalculateBackoff(int failedPublishCountBeforeThisFailure)
    {
        var exp = Math.Max(0, failedPublishCountBeforeThisFailure);
        var mult = Math.Pow(2, Math.Min(exp, 12));
        var seconds = _options.BaseRetrySeconds * mult;
        var capped = Math.Min(seconds, _options.MaxRetryDelaySeconds);
        return TimeSpan.FromSeconds(capped);
    }

    private static bool TryGetDocumentId(JsonElement root, out Guid documentId)
    {
        documentId = default;
        if (!root.TryGetProperty("documentId", out var el)) return false;
        return el.TryGetGuid(out documentId);
    }
}
