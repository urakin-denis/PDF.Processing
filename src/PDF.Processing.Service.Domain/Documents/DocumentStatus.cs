namespace PDF.Processing.Service.Domain.Documents;

public enum DocumentStatus
{
    Uploaded = 0,
    Queued = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    DeadLettered = 5
}

