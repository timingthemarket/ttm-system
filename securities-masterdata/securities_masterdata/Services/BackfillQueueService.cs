using System.Collections.Concurrent;

namespace securities_masterdata.Services;

public interface IBackfillQueueService
{
    void EnqueueBackfillRequest();
    bool TryDequeueBackfillRequest();
}

public class BackfillQueueService : IBackfillQueueService
{
    private readonly ConcurrentQueue<BackfillRequest> _queue = new();
    private readonly ILogger<BackfillQueueService> _logger;

    public BackfillQueueService(ILogger<BackfillQueueService> logger)
    {
        _logger = logger;
    }

    public void EnqueueBackfillRequest()
    {
        var request = new BackfillRequest(DateTime.UtcNow);
        _queue.Enqueue(request);
        _logger.LogInformation("Backfill request enqueued at {Timestamp}", request.Timestamp);
    }

    public bool TryDequeueBackfillRequest()
    {
        var result = _queue.TryDequeue(out var request);
        if (result && request != null)
        {
            _logger.LogInformation("Backfill request dequeued, was enqueued at {Timestamp}", request.Timestamp);
        }
        return result;
    }
}

public record BackfillRequest(DateTime Timestamp);