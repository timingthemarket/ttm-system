using System.Xml.Serialization;

namespace article_news_raw.DataAccess.Models;

public class UrlEntry
{
    [XmlElement("loc")] public string Location { get; set; }
}

[XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
public class UrlSet
{
    [XmlElement("url")] public List<UrlEntry> Urls { get; set; }
}