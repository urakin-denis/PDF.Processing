using PDF.Processing.Service.Api.Models.Pdfs;

namespace PDF.Processing.Service.Api.Services;

public interface IPdfApiService
{
    Task<UploadPdfResponse> UploadAsync(UploadPdfRequest request, CancellationToken ct);
    Task<ListPdfsResponse> ListAsync(ListPdfsRequest request, CancellationToken ct);
    Task<GetPdfResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<GetPdfTextResponse?> GetTextAsync(Guid id, CancellationToken ct);
}

