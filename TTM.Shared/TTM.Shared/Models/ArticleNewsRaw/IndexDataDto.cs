using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class IndexDataDto
{
    [DataMember(Order = 1, IsRequired = true)]
    public DateOnly Date { get; set; }

    /// <summary>
    /// One of <see cref="Constants.IndexTypes"/>.
    /// </summary>
    [DataMember(Order = 2, IsRequired = true)]
    public string IndexType { get; set; }

    /// <summary>
    /// Closing value of the index for <see cref="Date"/>.
    /// </summary>
    [DataMember(Order = 3, IsRequired = true)]
    public double Value { get; set; }
}
