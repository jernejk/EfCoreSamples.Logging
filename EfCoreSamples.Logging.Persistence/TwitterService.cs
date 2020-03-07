using EfCoreSamples.Logging.Persistence.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EfCoreSamples.Logging.Persistence
{
    public class TwitterService : ITwitterService
    {
        private readonly TwitterDbContext _context;
        private readonly ILogger<TwitterService> _logger;

        public TwitterService(TwitterDbContext context, ILogger<TwitterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Tweet>> GetTweets(CancellationToken ct = default)
        {
            // This will result following SQL query:
            // 
            //
            // No additional context is logged in rich logger.
            return await _context.Tweets
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Tweet>> GetTweetsWithQueryTags(CancellationToken ct = default)
        {
            // This will result following SQL query:
            // 
            //
            // No additional context is logged in rich logger.
            return await _context.Tweets
                .TagWith("GetTweets")
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Tweet>> GetTweetsWithExtraLogs(CancellationToken ct = default)
        {
            // This will result following SQL query:
            // 
            //
            // On top of that, it will also have property EFQueries with value "GetTweetsLog".
            using (_logger.EFQueryScope("GetTweetsLog"))
            {
                return await _context.Tweets
                    .TagWith("GetTweets + LogContext")
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
        }

        public async Task InsertTweet(string username, string message, CancellationToken ct = default)
        {
            using (_logger.EFQueryScope("InsertTweet"))
            {
                _context.Tweets.Add(new Tweet
                {
                    Username = username,
                    Message = message
                });

                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task InsertTweetStoreProc(string username, string message, CancellationToken ct = default)
        {
            using (_logger.EFQueryScope("InsertTweetStoreProc"))
            {
                _context.Tweets.FromSqlRaw(
                    "InsertTweet @Username, @Message",
                    new SqlParameter("Username", username),
                    new SqlParameter("Message", message));

                await _context.SaveChangesAsync(ct);
            }
        }
    }


    public interface ITwitterService
    {
        Task<IEnumerable<Tweet>> GetTweets(CancellationToken ct = default);
        Task<IEnumerable<Tweet>> GetTweetsWithQueryTags(CancellationToken ct = default);
        Task<IEnumerable<Tweet>> GetTweetsWithExtraLogs(CancellationToken ct = default);
        Task InsertTweet(string username, string message, CancellationToken ct = default);
        Task InsertTweetStoreProc(string username, string message, CancellationToken ct = default);
    }
}
