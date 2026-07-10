using Marten;
using Marten.Schema;
using riksbanken_raw.DataAccess.Models;

namespace riksbanken_raw.DataAccess.Seed;

public class SeedExchangeRateSeries : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();
        if (await session.Query<ExchangeRateSeries>().AnyAsync(cancellation))
            return;

        session.Store(
            new ExchangeRateSeries { SeriesId = "SEKEURPMI", Source = "Nasdaq", ShortDescription = "EUR", GroupId = 130 },
            new ExchangeRateSeries { SeriesId = "SEKNOKPMI", Source = "Nasdaq", ShortDescription = "NOK", GroupId = 130 },
            new ExchangeRateSeries { SeriesId = "SEKDKKPMI", Source = "Nasdaq", ShortDescription = "DKK", GroupId = 130 },
            new ExchangeRateSeries { SeriesId = "SEKUSDPMI", Source = "Nasdaq", ShortDescription = "USD", GroupId = 130 });

        await session.SaveChangesAsync(cancellation);
    }
}
