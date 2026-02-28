using System.Collections.Concurrent;
using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, ProcessingJob> _store = new();

    public ProcessingJob Create()
    {
        var job = new ProcessingJob();
        _store[job.Id] = job;
        return job;
    }

    public ProcessingJob? GetById(Guid id) =>
        _store.TryGetValue(id, out var job) ? job : null;

    public void Update(ProcessingJob job) =>
        _store[job.Id] = job;
}
