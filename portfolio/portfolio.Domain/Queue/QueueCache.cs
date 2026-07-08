using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace portfolio.Domain.Queue;

public interface IQueueCache<T>
{
    void Enqueue(T simulation);
    ImmutableList<T> GetQueueContents();
    int GetNumberQueuedItems();
    T? DequeueAndGetItem();
}

public sealed class QueueCache<T> : IQueueCache<T>
{
    private readonly ConcurrentQueue<T> _queue = new();

    public void Enqueue(T item) => _queue.Enqueue(item);

    public ImmutableList<T> GetQueueContents() => _queue.ToImmutableList();

    public int GetNumberQueuedItems() => _queue.Count;

    public T? DequeueAndGetItem()
    {
        if (_queue.TryDequeue(out var item))
            return item;

        return default;
    }
}