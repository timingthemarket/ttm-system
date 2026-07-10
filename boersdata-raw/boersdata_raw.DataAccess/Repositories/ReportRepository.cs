using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models.Report;
using Microsoft.EntityFrameworkCore;

namespace boersdata_raw.DataAccess.Repositories;

public class ReportRepository : IReportRepository
{
    public async Task SaveReportTypes(List<ReportTypes> types, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        await context.Database.ExecuteSqlAsync($"DELETE FROM report_types", token);

        foreach (var type in types)
            type.Id = 0;

        context.ReportTypes.AddRange(types);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
    }

    public async Task SaveHistoricalReports(string ticker, List<Report> reports, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        await context.Database.ExecuteSqlAsync($"DELETE FROM report WHERE ticker = {ticker}", token);

        // Reports read back via GetReports carry a populated Id; reset for identity insert
        foreach (var report in reports)
            report.Id = 0;

        context.Reports.AddRange(reports);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
    }

    public async Task<List<Report>> GetReports(string ticker, ReportType type, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Reports.AsNoTracking()
            .Where(r => r.Ticker == ticker && r.ReportType == type)
            .ToListAsync(token);
    }
}
