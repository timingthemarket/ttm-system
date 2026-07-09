using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Web;

namespace boersdata_raw.DataAccess.Extensions;
public static class HttpClientExtensions
{
    public static async Task<T?> GetJson<T>(this HttpClient client, string path, NameValueCollection? queryKeyValue = null,
        JsonTypeInfo<T>? typeInfo = null, CancellationToken token = default)
    {
        if (client.BaseAddress is null)
            throw new ArgumentNullException(nameof(client.BaseAddress), "Please provide a BaseAddress to use this method");
        
        var uri = GetUri(client.BaseAddress, path, queryKeyValue);
        string response = await client.GetStringAsync(uri, token);

        return typeInfo is null ? JsonSerializer.Deserialize<T>(response) : JsonSerializer.Deserialize<T>(response, typeInfo);
    }

    public static async Task<T?> GetJson<T>(this HttpClient client, Uri baseAddress, string path, NameValueCollection? queryKeyValue = null,
        JsonTypeInfo<T>? typeInfo = null, CancellationToken token = default)
    {
        var uri = GetUri(baseAddress, path, queryKeyValue);
        string response = await client.GetStringAsync(uri, token);

        return typeInfo is null
            ? JsonSerializer.Deserialize<T>(response)
            : JsonSerializer.Deserialize<T>(response, typeInfo);
    }

    private static Uri GetUri(Uri baseUrl, string path, NameValueCollection? parameters = null)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);

        if (parameters != null)
            query.Add(parameters);

        var builder = new UriBuilder($"{baseUrl}{path}")
        { Query = query.ToString() ?? string.Empty };
        return builder.Uri;
    }
}
