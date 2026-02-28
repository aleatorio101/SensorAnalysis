using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public interface IJobRepository
{
    ProcessingJob Create();
    ProcessingJob? GetById(Guid id);
    void Update(ProcessingJob job);
}
