using article_news_raw.Domain.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace article_news_raw.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class ArticleController(FetchNewsUrlsHandler fetchNewsUrlsHandler) : ControllerBase
{
    [HttpGet("trigger-url-fetch")]
    public async Task<IActionResult> TriggerUrlFetch([FromQuery] DateTime toDate)
    {
        await fetchNewsUrlsHandler.FetchNewsUrls(toDate);
        return Ok();
    }

    [HttpGet("trigger-url-fetch-range")]
    public async Task<IActionResult> TriggerUrlFetchRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var hours = new List<DateTime>();
        for (var date = fromDate; date <= toDate; date = date.AddHours(1))
            hours.Add(date);

        foreach (var chunk in hours.Chunk(5))
            await Task.WhenAll(chunk.Select(date => fetchNewsUrlsHandler.FetchNewsUrls(date)));

        return Ok();
    }
}