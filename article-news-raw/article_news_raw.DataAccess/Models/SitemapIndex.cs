using System.Xml.Serialization;

namespace article_news_raw.DataAccess.Models;


public class Sitemap
{
    [XmlElement("loc")] public string Location { get; set; }
}

// Root class that represents the entire sitemap index
[XmlRoot("sitemapindex", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
public class SitemapIndex
{
    [XmlElement("sitemap")] public List<Sitemap> Sitemaps { get; set; }
}