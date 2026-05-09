namespace PDF.Processing.Service.Api.Models.Pdfs;

public record ListPdfsRequest(
    int Page = 1,
    int PageSize = 50);

