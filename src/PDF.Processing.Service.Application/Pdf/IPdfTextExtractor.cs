namespace PDF.Processing.Service.Application.Pdf;

public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken);
}

