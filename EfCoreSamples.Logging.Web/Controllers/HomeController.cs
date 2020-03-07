using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EfCoreSamples.Logging.Web.Models;
using EfCoreSamples.Logging.Persistence;
using System.Threading;

namespace EfCoreSamples.Logging.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITwitterService _twitterService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ITwitterService twitterService, ILogger<HomeController> logger)
        {
            _twitterService = twitterService;
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromQuery] string logType = "", CancellationToken ct = default)
        {
            switch (logType)
            {
                case "query-tag":
                    _ = await _twitterService.GetTweetsWithQueryTags(ct);
                    break;
                case "all":
                    _ = await _twitterService.GetTweetsWithExtraLogs(ct);
                    break;

                case "scope-log":
                    using (_logger.EFQueryScope("GetTweets"))
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

        public async Task<IActionResult> GetTweets([FromQuery] string logType = "", CancellationToken ct = default)
        {
            var viewModel = new GetTweetsViewModel();
            viewModel.LogType = logType;
            switch (logType)
            {
                case "query-tag":
                    viewModel.LogTypeName = "Only Query Tags (WithTag)";
                    viewModel.ImagePath = "/img/efcore-logging-query-tag.png";
                    viewModel.Tweets = await _twitterService.GetTweetsWithQueryTags(ct);
                    break;
                case "all":
                    viewModel.LogTypeName = "Full logging";
                    viewModel.ImagePath = "/img/efcore-logging-full-log.png";
                    viewModel.Tweets = await _twitterService.GetTweetsWithExtraLogs(ct);
                    break;
                case "external":
                    viewModel.LogTypeName = "External logging";
                    viewModel.ImagePath = "/img/efcore-logging-external.png";

                    using (_logger.EFQueryScope("GetTweets"))
                    {
                        viewModel.Tweets = await _twitterService.GetTweets(ct);
                    }

                    break;

                default:
                    viewModel.LogTypeName = "No additional logs";
                    viewModel.ImagePath = "/img/efcore-logging-no-log.png";
                    viewModel.Tweets = await _twitterService.GetTweets(ct);
                    break;
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
