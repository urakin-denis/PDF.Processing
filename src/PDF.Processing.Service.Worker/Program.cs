using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PDF.Processing.Service.Infrastructure.DependencyInjection;
using PDF.Processing.Service.Infrastructure.Messaging.RabbitMq;
using PDF.Processing.Service.Infrastructure.Persistence;
using PDF.Processing.Service.Worker.Messaging;
using Serilog;

// Outbox relay runs here so pending messages are published to RabbitMQ without requiring the HTTP API process.
var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<OutboxPublisherWorker>();
builder.Services.AddHostedService<PdfProcessConsumerWorker>();

var host = builder.Build();

var rmqOpt = host.Services.GetRequiredService<IOptions<RabbitMqOptions>>();
Log.Information(
    "Worker will use RabbitMQ at {Host}:{Port} VHost={VHost}. If outbox publish fails with unreachable endpoints, fix broker reachability from this process (not the HTTP API).",
    rmqOpt.Value.Host,
    rmqOpt.Value.Port,
    rmqOpt.Value.VirtualHost);

await host.Services.ApplyDatabaseMigrationsAsync();
host.Run();
