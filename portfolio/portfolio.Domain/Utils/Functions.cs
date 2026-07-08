using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using portfolio.Domain.Serialization;

namespace portfolio.Domain.Utils;

public static class Functions
{
    // https://en.wikipedia.org/wiki/Feature_scaling#Rescaling_(min-max_normalization)
    public static double Normalize01(double value, double min, double max) => (value - min) / (max - min);
    public static decimal Normalize01(decimal value, decimal min, decimal max) => (value - min) / (max - min);

    public static double RescaleWithBounds(double value, double min, double max, double lb, double ub) => 
        lb +  (ub - lb) * (value - min) / (max - min);
    
    public static double TanhReversed(double scale, double value) =>
        scale + scale * -1 * Math.Tanh(value);

    public static double Standardise(double value, double mean, double std) =>
        (value - mean) / std;

    public static double StandardDeviation(List<double> values)
    {
        var mean = values.Average();
        var sumNoMean = values.Sum(v => v - mean);
        
        var top = sumNoMean / (values.Count - 1);
        
        return Math.Sqrt(top);
    }

    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody, int maxDoP = 4)
    {
        async Task AwaitPartition(IEnumerator<T> partition)
        {
            using (partition)
            {
                while (partition.MoveNext())
                {
                    await Task.Yield(); // prevents a sync/hot thread hangup
                    await funcBody(partition.Current);
                }
            }
        }

        return Task.WhenAll(
            Partitioner
                .Create(source)
                .GetPartitions(maxDoP)
                .AsParallel()
                .Select(AwaitPartition));
    }

    public static string GetObjectHash<T>(T obj, JsonTypeInfo<T>? options = null)
    {
        ReadOnlySpan<byte> bytes = options == null
            ? JsonSerializer.SerializeToUtf8Bytes(obj).AsSpan()
            : JsonSerializer.SerializeToUtf8Bytes(obj, options).AsSpan();

        Span<byte> hashBytes = stackalloc byte[16];
        MD5.HashData(bytes, hashBytes);
        return Convert.ToHexString(hashBytes);
    }
}

