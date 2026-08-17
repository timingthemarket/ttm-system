using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.Domain.Interfaces;
using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.Domain.Handlers.Query;

public class QryIndexDataHandler(IIndexDataRepository indexDataRepository) : IQryIndexDataHandler
{
    public async Task<List<IndexDataDto>> HandleGetIndexData(string indexType, DateOnly dateFrom, DateOnly dateTo,
        CancellationToken token = default)
    {
        var indexData = await indexDataRepository.GetIndexData(indexType, dateFrom, dateTo, token);
        return MapToIndexDataDtos(indexData);
    }

    private static List<IndexDataDto> MapToIndexDataDtos(List<IndexData> indexData) =>
        indexData
            .Select(i => new IndexDataDto
            {
                Date = i.Date,
                IndexType = i.IndexType,
                Value = i.Value
            })
            .ToList();
}
