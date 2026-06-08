using securities_masterdata.Domain.Models.Sync;

namespace securities_masterdata.Domain.Interfaces;

public interface IAvanzaSyncHandler
{
    Task<AvanzaSyncResult> HandleSyncSecuritiesWithAvanza(CancellationToken cancellationToken = default);
}