using System.Text;
using PDF.Processing.Service.Application.Pdf;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace PDF.Processing.Service.Infrastructure.Pdf;

internal class PdfPigTextExtractor : IPdfTextExtractor
{
    public Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(pdfStream);

        var sb = new StringBuilder(capacity: 16 * 1024);
        foreach (Page page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(ContentOrderTextExtractor.GetText(page));
        }

        return Task.FromResult(sb.ToString());
    }
}

