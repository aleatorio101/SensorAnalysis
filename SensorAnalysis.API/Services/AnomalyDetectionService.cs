using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public class AnomalyDetectionService : IAnomalyDetectionService
{
    private DescriptiveStats? _tempStats;
    private DescriptiveStats? _humStats;
    private DescriptiveStats? _dewStats;

    private readonly Dictionary<string, List<double>> _humidityBySensor = [];
    private HashSet<string> _staleSensors = [];

    public void Fit(IReadOnlyList<SensorReading> readings)
    {
        var validReadings = readings.Where(r =>
            r.temperature.HasValue && r.humidity.HasValue && r.dew_point.HasValue).ToList();

        if (validReadings.Count == 0) return;

        _tempStats = DescriptiveStats.From(validReadings.Select(r => r.temperature!.Value));
        _humStats  = DescriptiveStats.From(validReadings.Select(r => r.humidity!.Value));
        _dewStats  = DescriptiveStats.From(validReadings.Select(r => r.dew_point!.Value));

        foreach (var reading in validReadings)
        {
            if (!_humidityBySensor.TryGetValue(reading.sensor_id, out var list))
            {
                list = [];
                _humidityBySensor[reading.sensor_id] = list;
            }
            list.Add(reading.humidity!.Value);
        }

        _staleSensors = _humidityBySensor
            .Where(kvp => IsStale(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToHashSet();
    }

    public bool IsAnomaly(SensorReading reading)
    {

        if (!reading.temperature.HasValue || !reading.humidity.HasValue || !reading.dew_point.HasValue)
            return false;

        if (_tempStats is null || _humStats is null || _dewStats is null)
            return false;

        if (_staleSensors.Contains(reading.sensor_id))
            return true;

        if (IsZScoreAnomaly(reading.temperature.Value, _tempStats) ||
            IsZScoreAnomaly(reading.humidity.Value, _humStats)     ||
            IsZScoreAnomaly(reading.dew_point.Value, _dewStats))
            return true;

        if (IsIqrAnomaly(reading.temperature.Value, _tempStats) ||
            IsIqrAnomaly(reading.humidity.Value, _humStats)     ||
            IsIqrAnomaly(reading.dew_point.Value, _dewStats))
            return true;

        return false;
    }

    private static bool IsZScoreAnomaly(double value, DescriptiveStats stats) =>
        stats.StdDev > 0 && Math.Abs((value - stats.Mean) / stats.StdDev) > 3.0;

    private static bool IsIqrAnomaly(double value, DescriptiveStats stats)
    {
        var iqr   = stats.Q3 - stats.Q1;
        var lower = stats.Q1 - 1.5 * iqr;
        var upper = stats.Q3 + 1.5 * iqr;
        return value < lower || value > upper;
    }

    private static bool IsStale(List<double> values)
    {
        if (values.Count < 5) return false;
        var mode  = values.GroupBy(v => v).OrderByDescending(g => g.Count()).First();
        return (double)mode.Count() / values.Count >= 0.80;
    }
}

internal sealed class DescriptiveStats
{
    public double Mean   { get; private init; }
    public double StdDev { get; private init; }
    public double Q1     { get; private init; }
    public double Q3     { get; private init; }

    public static DescriptiveStats From(IEnumerable<double> source)
    {
        var sorted = source.OrderBy(v => v).ToArray();
        var n      = sorted.Length;

        double mean   = sorted.Average();
        double stdDev = Math.Sqrt(sorted.Sum(v => Math.Pow(v - mean, 2)) / n);

        return new DescriptiveStats
        {
            Mean   = mean,
            StdDev = stdDev,
            Q1     = Percentile(sorted, 0.25),
            Q3     = Percentile(sorted, 0.75)
        };
    }

    private static double Percentile(double[] sorted, double p)
    {
        double index = p * (sorted.Length - 1);
        int    lower = (int)Math.Floor(index);
        int    upper = (int)Math.Ceiling(index);
        double frac  = index - lower;
        return upper >= sorted.Length
            ? sorted[lower]
            : sorted[lower] + frac * (sorted[upper] - sorted[lower]);
    }
}
