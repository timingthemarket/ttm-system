using System.Collections.Concurrent;
using System.Collections.Immutable;
using portfolio.Domain.Models;

namespace portfolio.Domain.Queue;

public sealed class SimulationQueueCache
{
    private readonly ConcurrentQueue<ProcessSimulation> _queue = new();

    public void Enqueue(ProcessSimulation simulation) => _queue.Enqueue(simulation);

    public ImmutableList<ProcessSimulation> GetQueueContents() => _queue.ToImmutableList();

    public int GetNumberQueuedItems() => _queue.Count;
    
    public ProcessSimulation? DequeueAndGetItem()
    {
        if (_queue.TryDequeue(out var item))
            return item;

        return null;
    }
}