using Microsoft.AspNetCore.Mvc;
using PDF.Processing.Service.Api.Models.Pdfs;
using PDF.Processing.Service.Api.Services;

namespace PDF.Processing.Service.Api.Controllers;

[ApiController]
[Route("api/v1/pdfs")]
public class PdfsController : ControllerBase
{
    private readonly IPdfApiService _pdfApiService;

    public PdfsController(IPdfApiService pdfApiService)
    {
        _pdfApiService = pdfApiService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadPdfResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> Upload([FromForm] UploadPdfRequest request, CancellationToken ct)
    {
        var response = await _pdfApiService.UploadAsync(request, ct);
        var location = $"/api/v1/pdfs/{response.DocumentId}";
        return Accepted(location, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListPdfsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListPdfsResponse>> List([FromQuery] ListPdfsRequest request, CancellationToken ct = default)
    {
        var response = await _pdfApiService.ListAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPdfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetPdfResponse>> Get([FromRoute] Guid id, CancellationToken ct)
    {
        var response = await _pdfApiService.GetAsync(id, ct);
        if (response is null) return NotFound();
        return Ok(response);
    }

    [HttpGet("{id:guid}/text")]
    [ProducesResponseType(typeof(GetPdfTextResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetPdfTextResponse>> GetText([FromRoute] Guid id, CancellationToken ct)
    {
        var response = await _pdfApiService.GetTextAsync(id, ct);
        if (response is null) return NotFound();
        return Ok(response);
    }
}

