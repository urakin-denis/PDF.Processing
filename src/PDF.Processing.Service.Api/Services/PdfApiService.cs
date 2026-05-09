using Microsoft.Extensions.Options;
using PDF.Processing.Service.Api.Models.Pdfs;
using PDF.Processing.Service.Application.Storage;
using PDF.Processing.Service.Application.Pdf;
using PDF.Processing.Service.Contracts.Messaging;

namespace PDF.Processing.Service.Api.Services;

public class PdfApiService : IPdfApiService
{
    private readonly IPdfUploadService _uploadService;
    private readonly IPdfQueryService _queryService;

    public PdfApiService(
        IPdfUploadService uploadService,
        IPdfQueryService queryService)
    {
        _uploadService = uploadService;
        _queryService = queryService;
    }

    public async Task<UploadPdfResponse> UploadAsync(UploadPdfRequest request, CancellationToken ct)
    {
        var file = request.File;

        var now = DateTimeOffset.UtcNow;
        await using var stream = file.OpenReadStream();

        var result = await _uploadService.UploadAsync(
            new UploadPdfInput(
                file.FileName,
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/pdf" : file.ContentType,
                file.Length,
                stream),
            now,
            ct);

        return new UploadPdfResponse(result.DocumentId, result.Status.ToString());
    }

    public async Task<ListPdfsResponse> ListAsync(ListPdfsRequest request, CancellationToken ct)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var items = await _queryService.ListAsync(page, pageSize, ct);
        return new ListPdfsResponse(items.Select(x => new PdfListItemResponse(
            x.Id,
            x.FileName,
            x.Status,
            x.CreatedAtUtc,
            x.UpdatedAtUtc)).ToList());
    }

    public async Task<GetPdfResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var doc = await _queryService.GetAsync(id, ct);
        if (doc is null) return null;

        return new GetPdfResponse(
            doc.Id,
            doc.FileName,
            doc.Bucket,
            doc.ObjectKey,
            doc.SizeBytes,
            doc.ContentType,
            doc.Status,
            doc.CreatedAtUtc,
            doc.UpdatedAtUtc,
            doc.ProcessingStartedAtUtc,
            doc.ProcessedAtUtc,
            doc.LastError,
            doc.AttemptCount);
    }

    public async Task<GetPdfTextResponse?> GetTextAsync(Guid id, CancellationToken ct)
    {
        var text = await _queryService.GetTextIfSucceededAsync(id, ct);
        if (text is null) return null;
        return new GetPdfTextResponse(id, text);
    }
}

