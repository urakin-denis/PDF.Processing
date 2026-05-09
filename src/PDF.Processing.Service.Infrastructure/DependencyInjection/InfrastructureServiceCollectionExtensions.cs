using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using PDF.Processing.Service.Application.Storage;
using PDF.Processing.Service.Application.Messaging;
using PDF.Processing.Service.Application.Pdf;
using PDF.Processing.Service.Application.Interfaces;
using PDF.Processing.Service.Infrastructure.Messaging.RabbitMq;
using PDF.Processing.Service.Infrastructure.Pdf;
using PDF.Processing.Service.Infrastructure.Persistence;
using PDF.Processing.Service.Infrastructure.Persistence.Repositories;
using PDF.Processing.Service.Infrastructure.Storage.Minio;

namespace PDF.Processing.Service.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var cs = configuration.GetConnectionString("Postgres");
            options.UseNpgsql(cs);
        });

        services.AddOptions<OutboxPublishingOptions>()
            .Bind(configuration.GetSection(OutboxPublishingOptions.SectionName))
            .Validate(o => o.MaxRetries > 0 && o.BaseRetrySeconds > 0 && o.MaxRetryDelaySeconds >= o.BaseRetrySeconds, "OutboxPublishing: MaxRetries, BaseRetrySeconds, MaxRetryDelaySeconds are invalid");

        services.AddScoped<IPdfDocumentRepository, PdfDocumentRepository>();
        services.AddScoped<IPdfProcessingRepository, PdfProcessingRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddScoped<IPdfUploadService, PdfUploadService>();
        services.AddScoped<IPdfQueryService, PdfQueryService>();
        services.AddScoped<IPdfProcessService, PdfProcessService>();
        services.AddScoped<IOutboxPublishService, OutboxPublishService>();

        services.AddMinio(configuration);
        services.AddRabbitMq(configuration);
        services.AddPdf(configuration);

        return services;
    }

    private static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MinioOptions>()
            .Bind(configuration.GetSection(MinioOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "Minio:Endpoint is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.AccessKey), "Minio:AccessKey is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.SecretKey), "Minio:SecretKey is required");

        services.AddSingleton<IMinioClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MinioOptions>>().Value;

            var endpoint = opt.Endpoint;
            if (endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                endpoint = endpoint["http://".Length..];
            if (endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                endpoint = endpoint["https://".Length..];

            var client = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(opt.AccessKey, opt.SecretKey);

            if (opt.UseSsl)
                client = client.WithSSL();

            return client.Build();
        });

        services.AddScoped<IObjectStorageService, MinioObjectStorage>();
        services.AddSingleton<IObjectStorageBucketProvider, MinioBucketProvider>();

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "RabbitMq:Host is required");

        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<IMessageRoutingProvider, RabbitMqRoutingProvider>();
        return services;
    }

    private static IServiceCollection AddPdf(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PdfProcessingOptions>()
            .Bind(configuration.GetSection(PdfProcessingOptions.SectionName));

        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
        return services;
    }
}

