using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class IndexDataQry
{
    /// <summary>
    /// The index to fetch, one of <see cref="Constants.IndexTypes"/>.
    /// </summary>
    [DataMember(Order = 1, IsRequired = true)]
    public string IndexType { get; set; }

    /// <summary>
    /// Inclusive lower bound of the period to fetch.
    /// </summary>
    [DataMember(Order = 2, IsRequired = true)]
    public DateOnly DateFrom { get; set; }

    /// <summary>
    /// Inclusive upper bound of the period to fetch.
    /// </summary>
    [DataMember(Order = 3, IsRequired = true)]
    public DateOnly DateTo { get; set; }
}
