namespace SensorAnalysis.API.DTOs;

public record StartAnalysisResponse(Guid JobId, string Message);

public record JobProgressResponse(
    Guid JobId,
    string Status,
    int TotalSamples,
    int ProcessedSamples,
    double ProgressPercent,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage
);
