namespace PDF.Processing.Service.Domain.Documents;

public class DocumentStatusLookup
{
    public DocumentStatus Id { get; private set; }
    public string Name { get; private set; } = default!;

    private DocumentStatusLookup() { }

    public DocumentStatusLookup(DocumentStatus id, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Id = id;
        Name = name;
    }
}

