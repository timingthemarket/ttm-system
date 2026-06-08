using System.IO.Compression;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using article_news_raw.DataAccess.Models;

namespace article_news_raw.DataAccess.Services;

public class SitemapService
{
    private readonly HttpClient _httpClient;
    
    public SitemapService()
    {
        var handler = new HttpClientHandler();
        handler.UseCookies = true;
        handler.CookieContainer = new CookieContainer();

        _httpClient = new HttpClient(handler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 15.7; rv:145.0) Gecko/20100101 Firefox/145.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
    }
    
    public async Task<Stream> GetSitemap(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var contentEncoding = response.Content.Headers.ContentEncoding;
            
            if (contentEncoding.Contains("gzip"))
            {
                var compressedStream = await response.Content.ReadAsStreamAsync();
                return new GZipStream(compressedStream,
                    CompressionMode.Decompress);
            }
            
            if (contentEncoding.Contains("deflate"))
            {
                var compressedStream = await response.Content.ReadAsStreamAsync();
                return new DeflateStream(compressedStream,
                    CompressionMode.Decompress);
            }
            
            if (contentEncoding.Contains("br"))
            {
                var compressedStream = await response.Content.ReadAsStreamAsync();
                return new BrotliStream(compressedStream,
                    CompressionMode.Decompress);
            }

            return await response.Content.ReadAsStreamAsync();
        }
        
        throw new Exception($"Failed to fetch sitemap from {url}. Status code: {response.StatusCode}");
    }

    public async Task<List<string>> GetArticlesFromSitemap(string url, int maxRootSitemaps)
    {
        var sitemapStream = await GetSitemap(url);
        using var reader = new StreamReader(sitemapStream, Encoding.UTF8);
        var xmlString = await reader.ReadToEndAsync();
        
        var urlList = new List<string>();
        var xml = TryParseSitemapIndex(xmlString);

        if (xml != null)
        {
            foreach (var sitemap in xml.Sitemaps.Take(maxRootSitemaps))
            {
                if (!IsSitemap(sitemap.Location)) continue;
                
                var result = await GetArticlesFromSitemap(sitemap.Location, maxRootSitemaps);
                urlList.AddRange(result);
            }
        }
        else
        {
            var urlSet = TryParseUrlSet(xmlString);
            if (urlSet == null) return urlList;
            foreach (var urlEntry in urlSet.Urls)
            {
                if (IsSitemap(urlEntry.Location)) continue;
                urlList.AddRange(urlEntry.Location);
            }
        }
        
        return urlList;
    }

    private static SitemapIndex? TryParseSitemapIndex(string sitemapStream)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(SitemapIndex));
            using var reader = new StringReader(sitemapStream);
            return (SitemapIndex?)serializer.Deserialize(reader);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static UrlSet? TryParseUrlSet(string urlSetStream)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(UrlSet));
            using var reader = new StringReader(urlSetStream);
            return (UrlSet?)serializer.Deserialize(reader);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsSitemap(string url)
    {
        return url.EndsWith(".xml");
    }
}