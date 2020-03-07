using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
