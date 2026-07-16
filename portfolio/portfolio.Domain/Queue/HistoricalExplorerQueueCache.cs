using System.Collections.Concurrent;
using System.Collections.Immutable;
using portfolio.Domain.Models;

namespace portfolio.Domain.Queue;

public sealed class HistoricalExplorerQueueCache
{
    private readonly ConcurrentQueue<HistoricalExplorerCalculationRequest> _queue = new();
    private readonly ConcurrentDictionary<string, CurrentlyRunningRequest> _currentlyRunning = new();

    public void Enqueue(HistoricalExplorerCalculationRequest request) => _queue.Enqueue(request);

    public ImmutableList<HistoricalExplorerCalculationRequest> GetQueueContents() => _queue.ToImmutableList();

    public int GetNumberQueuedItems() => _queue.Count;
    
    public HistoricalExplorerCalculationRequest? DequeueAndGetItem()
    {
        if (_queue.TryDequeue(out var item))
            return item;

        return null;
    }

    public string SetCurrentlyRunning(HistoricalExplorerCalculationRequest request)
    {
        var key = $"{request.SessionDate:yyyy-MM-dd}_{Guid.NewGuid():N}";
        var runningRequest = new CurrentlyRunningRequest(request, DateTime.UtcNow, key);
        _currentlyRunning.TryAdd(key, runningRequest);
        return key;
    }
    
    public void UpdateCurrentlyRunning(string key, int doneIterations)
    {
        if (_currentlyRunning.TryGetValue(key, out var current))
        {
            _currentlyRunning.TryUpdate(key, current with { NrIterationsDone = doneIterations }, current);
        }
    }

    public void ClearCurrentlyRunning(string key)
    {
        _currentlyRunning.TryRemove(key, out _);
    }

    public ImmutableList<CurrentlyRunningRequest> GetCurrentlyRunning()
    {
        return _currentlyRunning.Values.ToImmutableList();
    }

    public CurrentlyRunningRequest? GetCurrentlyRunningBySessionDate(DateOnly sessionDate)
    {
        return _currentlyRunning.Values.FirstOrDefault(r => r.Request.SessionDate == sessionDate);
    }
}

public record CurrentlyRunningRequest(
    HistoricalExplorerCalculationRequest Request,
    DateTime ProcessingStartedAt,
    string Key)
{
    public int NrIterationsDone { get; set; }
    public TimeSpan ProcessingDuration => DateTime.UtcNow - ProcessingStartedAt;
}