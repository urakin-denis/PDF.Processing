using Microsoft.Extensions.Options;
using PDF.Processing.Service.Application.Messaging;

namespace PDF.Processing.Service.Infrastructure.Messaging.RabbitMq;

internal class RabbitMqRoutingProvider : IMessageRoutingProvider
{
    private readonly RabbitMqOptions _options;

    public RabbitMqRoutingProvider(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public (string Exchange, string RoutingKey) GetOutboxRouting() => (_options.Exchange, _options.RoutingKey);
}

