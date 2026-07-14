using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models.Report;
using Marten;

namespace boersdata_raw.DataAccess.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IDocumentStore _store;

    public ReportRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task SaveReportTypes(List<ReportTypes> types, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<ReportTypes>(t => true);
        await session.SaveChangesAsync(token);

        session.Store(types.ToArray());
        await session.SaveChangesAsync(token);
    }

    public async Task SaveHistoricalReports(string ticker, List<Report> reports, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Report>(r => r.Ticker == ticker);
        await session.SaveChangesAsync(token);

        session.Store(reports.ToArray());
        await session.SaveChangesAsync(token);
    }

    public async Task<List<Report>> GetReports(string ticker, ReportType type, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var reports = await session.Query<Report>()
            .Where(r => r.Ticker == ticker && r.ReportType == type)
            .ToListAsync(token);
        return reports.ToList();
    }
}
