using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PDF.Processing.Service.Application.Messaging;
using RabbitMQ.Client;

namespace PDF.Processing.Service.Infrastructure.Messaging.RabbitMq;

internal class RabbitMqPublisher : IMessagePublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options;
    private readonly RabbitMqConnectionFactory _factory;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, RabbitMqConnectionFactory factory)
    {
        _options = options.Value;
        _factory = factory;
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken)
    {
        var connectionFactory = _factory.Create();
        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await RabbitMqTopology.DeclareAsync(channel, _options, cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, JsonOptions));
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }
}

