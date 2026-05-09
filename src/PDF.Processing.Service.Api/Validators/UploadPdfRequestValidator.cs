using FluentValidation;
using Microsoft.Extensions.Options;
using PDF.Processing.Service.Api.Models.Pdfs;
using PDF.Processing.Service.Application.Pdf;

namespace PDF.Processing.Service.Api.Validators;

public class UploadPdfRequestValidator : AbstractValidator<UploadPdfRequest>
{
    public const string FileTooLargeErrorCode = "FileTooLarge";

    public UploadPdfRequestValidator(IOptions<PdfProcessingOptions> processingOptions)
    {
        var maxBytes = (long)processingOptions.Value.MaxFileSizeMb * 1024 * 1024;

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File is empty");

            RuleFor(x => x.File.Length)
                .LessThanOrEqualTo(maxBytes)
                .WithErrorCode(FileTooLargeErrorCode)
                .WithMessage("File too large");

            RuleFor(x => x.File.ContentType)
                .NotEmpty()
                .Equal("application/pdf")
                .WithMessage("Only PDF files are allowed");

            RuleFor(x => x.File.FileName)
                .NotEmpty()
                .Must(fileName => Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Only PDF files are allowed");
        });
    }
}

