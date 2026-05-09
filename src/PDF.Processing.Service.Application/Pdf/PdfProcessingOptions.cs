namespace PDF.Processing.Service.Application.Pdf;

public class PdfProcessingOptions
{
    public const string SectionName = "PdfProcessing";

    public int MaxAttempts { get; init; } = 5;
    public int MaxFileSizeMb { get; init; } = 50;
}

