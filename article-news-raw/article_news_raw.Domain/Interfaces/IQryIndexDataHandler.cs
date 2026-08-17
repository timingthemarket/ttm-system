using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.Domain.Interfaces;

public interface IQryIndexDataHandler
{
    Task<List<IndexDataDto>> HandleGetIndexData(string indexType, DateOnly dateFrom, DateOnly dateTo, CancellationToken token = default);
}
