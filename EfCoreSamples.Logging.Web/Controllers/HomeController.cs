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

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTweets([FromQuery] string logType = "", CancellationToken ct = default)
        {
            var viewModel = new GetTweetsViewModel();
            viewModel.LogType = logType;
            if (string.IsNullOrWhiteSpace(logType))
            {
                viewModel.LogTypeName = "No additional logs";
                viewModel.Tweets = await _twitterService.GetTweets(ct);
            }
            else if (logType == "query-tag")
            {
                viewModel.LogTypeName = "Only Query Tags (WithTag)";
                viewModel.Tweets = await _twitterService.GetTweetsWithQueryTags(ct);
            }
            else if (logType == "all")
            {
                viewModel.LogTypeName = "Full logging";
                viewModel.Tweets = await _twitterService.GetTweetsWithExtraLogs(ct);
            }
            else if (logType == "external")
            {
                viewModel.LogTypeName = "External logging";

                using (_logger.EFQueryScope("GetTweets"))
                {
                    viewModel.Tweets = await _twitterService.GetTweets(ct);
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
