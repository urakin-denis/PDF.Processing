using System.Text.Json;
using PDF.Processing.Service.Application.Interfaces;
using PDF.Processing.Service.Application.Storage;
using PDF.Processing.Service.Contracts.Messaging;
using PDF.Processing.Service.Domain.Documents;

namespace PDF.Processing.Service.Application.Pdf;

public class PdfUploadService : IPdfUploadService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IPdfDocumentRepository _repository;
    private readonly IObjectStorageService _objectStorage;
    private readonly IObjectStorageBucketProvider _bucketProvider;

    public PdfUploadService(
        IPdfDocumentRepository repository,
        IObjectStorageService objectStorage,
        IObjectStorageBucketProvider bucketProvider)
    {
        _repository = repository;
        _objectStorage = objectStorage;
        _bucketProvider = bucketProvider;
    }

    public async Task<UploadPdfResult> UploadAsync(UploadPdfInput input, DateTimeOffset nowUtc, CancellationToken ct)
    {
        var documentId = Guid.NewGuid();
        var bucket = _bucketProvider.GetBucket();
        var objectKey = $"{documentId:N}.pdf";
        var contentType = string.IsNullOrWhiteSpace(input.ContentType) ? "application/pdf" : input.ContentType;

        await _objectStorage.PutObjectAsync(bucket, objectKey, input.Content, input.SizeBytes, contentType, ct);

        var doc = new Document(
            documentId,
            input.FileName,
            bucket,
            objectKey,
            input.SizeBytes,
            contentType,
            nowUtc);

        var msg = new PdfProcessRequested(
            doc.Id,
            doc.Bucket,
            doc.ObjectKey,
            doc.FileName,
            doc.ContentType,
            nowUtc);

        var payloadJson = JsonSerializer.Serialize(msg, JsonOptions);
        await _repository.AddUploadedAsync(doc, nowUtc, nameof(PdfProcessRequested), payloadJson, ct);

        return new UploadPdfResult(doc.Id, doc.Status);
    }
}

