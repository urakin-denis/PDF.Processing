using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PDF.Processing.Service.Contracts.Messaging;
using PDF.Processing.Service.Application.Pdf;
using PDF.Processing.Service.Infrastructure.Messaging.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PDF.Processing.Service.Worker.Messaging;

internal class PdfProcessConsumerWorker : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<PdfProcessConsumerWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqOptions _options;
    private readonly RabbitMqConnectionFactory _factory;

    public PdfProcessConsumerWorker(
        ILogger<PdfProcessConsumerWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<RabbitMqOptions> options,
        RabbitMqConnectionFactory factory)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer loop crashed; retrying in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        var connectionFactory = _factory.Create();
        await using var connection = await connectionFactory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await RabbitMqTopology.DeclareAsync(channel, _options, stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var bodyCopy = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(bodyCopy);

                _logger.LogInformation("Received message {DeliveryTag} redelivered={Redelivered}", args.DeliveryTag, args.Redelivered);

                var msg = JsonSerializer.Deserialize<PdfProcessRequested>(json, JsonOptions)
                          ?? throw new InvalidOperationException("Failed to deserialize PdfProcessRequested");

                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IPdfProcessService>();

                var outcome = await processor.ProcessAsync(msg, stoppingToken);
                switch (outcome)
                {
                    case PdfProcessOutcome.Succeeded:
                        await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        break;
                    case PdfProcessOutcome.Retry:
                        await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                        break;
                    case PdfProcessOutcome.NotFound:
                    case PdfProcessOutcome.DeadLetter:
                        await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message; Nack requeue=true");
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        var consumerTag = await channel.BasicConsumeAsync(
            queue: _options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMQ consumer started, tag={ConsumerTag}", consumerTag);

        var tcs = new TaskCompletionSource();
        stoppingToken.Register(() => tcs.TrySetResult());
        await tcs.Task;
    }
}

