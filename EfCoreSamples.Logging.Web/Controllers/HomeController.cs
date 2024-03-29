﻿using Microsoft.AspNetCore.Mvc;
using EfCoreSamples.Logging.Persistence;

namespace EfCoreSamples.Logging.Web.Controllers;

public class HomeController : Controller
{
    private readonly ITwitterService _twitterService;
    private readonly ILogger _logger;

    public HomeController(ITwitterService twitterService, ILogger<HomeController> logger)
    {
        _twitterService = twitterService;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] string logType = "", CancellationToken ct = default)
    {
        // NOTE: All results are intentionally discarded.
        // The purpose is to see the logs that they generate.
        switch (logType)
        {
            case "query-tag":
                _ = await _twitterService.GetTweetsWithQueryTags(ct);
                break;

            case "all":
                _ = await _twitterService.GetTweetsWithExtraLogs(ct);
                break;

            case "all-sql":
                _ = await _twitterService.GetTweetsWithExtraLogsAsSql(ct);
                break;

            case "scope-log":
                using (_logger.QueryScope("GetTweets"))
                {
                    _ = await _twitterService.GetTweets(ct);
                }
                break;

            case "scope-log-insert":
                await _twitterService.InsertTweet("jk", "Insert with Log Scope.", ct);
                break;

            case "scope-log-proc":
                await _twitterService.InsertTweetStoreProc("jk", "Store procedure insert.", ct);
                break;

            case "no-log":
                _ = await _twitterService.GetTweets(ct);
                break;

            case "insert-no-log":
                await _twitterService.InsertTweetWithoutLogScope("jk", "Traditional insert.", ct);
                break;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
