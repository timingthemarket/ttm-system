using System;
using System.Threading;
using System.Threading.Tasks;

namespace portfolio.Domain.Interfaces;

public interface IHistoricalExplorerHandler
{
    Task<bool> ProcessHistoricalExplorerFromQueue(CancellationToken cancellationToken = default);
}