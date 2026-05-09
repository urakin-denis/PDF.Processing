namespace PDF.Processing.Service.Application.Messaging;

public interface IMessageRoutingProvider
{
    (string Exchange, string RoutingKey) GetOutboxRouting();
}

