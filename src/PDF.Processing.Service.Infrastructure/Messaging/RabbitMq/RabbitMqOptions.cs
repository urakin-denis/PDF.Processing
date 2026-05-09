namespace PDF.Processing.Service.Infrastructure.Messaging.RabbitMq;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";

    public string Exchange { get; init; } = "pdf.processing";
    public string DlxExchange { get; init; } = "pdf.processing.dlx";

    public string Queue { get; init; } = "pdf.process";
    public string DlqQueue { get; init; } = "pdf.process.dlq";

    public string RoutingKey { get; init; } = "pdf.process.requested";
    public string DlqRoutingKey { get; init; } = "pdf.process.dead";

    public ushort PrefetchCount { get; init; } = 10;
}

