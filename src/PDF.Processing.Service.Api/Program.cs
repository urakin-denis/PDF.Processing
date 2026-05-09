using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using PDF.Processing.Service.Api.Models.Pdfs;
using PDF.Processing.Service.Api.Services;
using PDF.Processing.Service.Api.Validators;
using PDF.Processing.Service.Infrastructure.DependencyInjection;
using PDF.Processing.Service.Infrastructure.Persistence;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UploadPdfRequestValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        var firstMessage = errors.FirstOrDefault() ?? "Validation failed";

        if (string.Equals(firstMessage, "File too large", StringComparison.OrdinalIgnoreCase))
        {
            return new ObjectResult(new ErrorResponse(firstMessage))
            {
                StatusCode = StatusCodes.Status413PayloadTooLarge
            };
        }

        return new BadRequestObjectResult(new ErrorResponse(firstMessage));
    };
});

builder.Services.AddScoped<IPdfApiService, PdfApiService>();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.ApplyDatabaseMigrationsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("N")[..8];

    context.Response.Headers["X-Correlation-ID"] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.MapControllers();

app.Run();
