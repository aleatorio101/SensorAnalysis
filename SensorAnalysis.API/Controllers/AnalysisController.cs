using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SensorAnalysis.API.DTOs;
using SensorAnalysis.API.Models;
using SensorAnalysis.API.Services;

namespace SensorAnalysis.API.Controllers;

[ApiController]
[Route("api/analysis")]
[Produces("application/json")]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisOrchestrator _orchestrator;
    private readonly IJobRepository        _jobs;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        IAnalysisOrchestrator orchestrator,
        IJobRepository        jobs,
        ILogger<AnalysisController> logger)
    {
        _orchestrator = orchestrator;
        _jobs         = jobs;
        _logger       = logger;
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(StartAnalysisResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided or file is empty." });

        if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only .json files are accepted." });

        List<SensorReading> readings;

        try
        {
            await using var stream = file.OpenReadStream();
            readings = await JsonSerializer.DeserializeAsync<List<SensorReading>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct)
                ?? throw new InvalidDataException("File deserialized to null.");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON file uploaded.");
            return BadRequest(new { error = "Invalid JSON format.", detail = ex.Message });
        }

        if (readings.Count == 0)
            return BadRequest(new { error = "JSON file contains no samples." });

        var jobId = await _orchestrator.StartAsync(readings, ct);

        return Accepted(new StartAnalysisResponse(jobId, $"Analysis started for {readings.Count} samples."));
    }


    [HttpGet("{jobId:guid}/progress")]
    [ProducesResponseType(typeof(JobProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProgress(Guid jobId)
    {
        var job = _jobs.GetById(jobId);
        if (job is null) return NotFound(new { error = $"Job '{jobId}' not found." });

        return Ok(new JobProgressResponse(
            job.Id,
            job.Status.ToString(),
            job.TotalSamples,
            job.ProcessedSamples,
            job.ProgressPercent,
            job.CreatedAt,
            job.CompletedAt,
            job.ErrorMessage));
    }

    [HttpGet("{jobId:guid}/results")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult GetResults(Guid jobId, [FromQuery] string? type = null)
    {
        var job = _jobs.GetById(jobId);
        if (job is null) return NotFound(new { error = $"Job '{jobId}' not found." });

        if (job.Status != JobStatus.Completed)
            return Conflict(new
            {
                error  = "Job has not completed yet.",
                status = job.Status.ToString(),
                progress = job.ProgressPercent
            });

        var results = string.IsNullOrWhiteSpace(type)
            ? job.Results
            : job.Results.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(new
        {
            jobId    = job.Id,
            summary  = job.Summary,
            results
        });
    }

    [HttpGet("{jobId:guid}/summary")]
    [ProducesResponseType(typeof(DashboardSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult GetSummary(Guid jobId)
    {
        var job = _jobs.GetById(jobId);
        if (job is null) return NotFound(new { error = $"Job '{jobId}' not found." });

        if (job.Status != JobStatus.Completed)
            return Conflict(new { error = "Job has not completed yet.", status = job.Status.ToString() });

        return Ok(job.Summary);
    }
}
