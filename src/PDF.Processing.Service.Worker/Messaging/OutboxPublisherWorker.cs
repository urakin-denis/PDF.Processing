using PDF.Processing.Service.Application.Messaging;
namespace PDF.Processing.Service.Worker.Messaging;
internal class OutboxPublisherWorker : BackgroundService
{
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IServiceProvider _services;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);
    public OutboxPublisherWorker(ILogger<OutboxPublisherWorker> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher iteration failed");
            }
            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
    private async Task PublishPendingAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxPublishService>();
        await outbox.PublishPendingAsync(ct);
    }
}