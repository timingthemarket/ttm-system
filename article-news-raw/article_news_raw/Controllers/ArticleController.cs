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
}