using System;
using System.Collections.Generic;
using System.Linq;
using securities_masterdata.DataAccess.Entities;

namespace securities_masterdata.Tests;

public class Helpers
{
    public static List<SecurityPrice> GenerateSecurityPrices(List<long> securityIds, int nrRows, DateOnly date)
    {
        var rnd = new Random();
        return securityIds.SelectMany(sId => Enumerable.Range(0, nrRows).Select(i => new SecurityPrice
        {
            SecurityId = sId,
            Date = date.AddDays(-i),
            Volume = rnd.NextInt64(),
            Close = rnd.NextDouble(),
            High = rnd.NextDouble(),
            Low = rnd.NextDouble(),
            Open = rnd.NextDouble()
        })).ToList();
    }
}