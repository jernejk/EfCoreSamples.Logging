using EfCoreSamples.Logging.Persistence.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //  SELECT[t].[Id], [t].[CreatedUtc], [t].[Message], [t].[Username]
            //  FROM[Tweets] AS[t]
            //
            // No additional context is logged in rich logger.
            return await _context.Tweets
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Tweet>> GetTweetsWithQueryTags(CancellationToken ct = default)
        {
            // This will result following SQL query:
            //  -- GetTweets
            //
            //  SELECT[t].[Id], [t].[CreatedUtc], [t].[Message], [t].[Username]
            //  FROM[Tweets] AS[t]
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
            //  --GetTweets + LogContext
            //  
            //  SELECT[t].[Id], [t].[CreatedUtc], [t].[Message], [t].[Username]
            //  FROM[Tweets] AS[t]
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

        public async Task InsertTweetWithoutLogScope(string username, string message, CancellationToken ct = default)
        {
            _context.Tweets.Add(new Tweet
            {
                Username = username,
                Message = message,
                CreatedUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
        }

        public async Task InsertTweet(string username, string message, CancellationToken ct = default)
        {
            using (_logger.EFQueryScope("InsertTweet"))
            {
                _context.Tweets.Add(new Tweet
                {
                    Username = username,
                    Message = message,
                    CreatedUtc = DateTime.UtcNow
                });

                // Queries are happening here and they are batched together into a single SQL request.
                await _context.SaveChangesAsync(ct);
            }
        }

        public Task InsertTweetStoreProc(string username, string message, CancellationToken ct = default)
        {
            using (_logger.EFQueryScope("InsertTweetStoreProc"))
            {
                _ = _context.Tweets
                    .FromSqlRaw(
                        "InsertTweet @Username, @Message",
                        new SqlParameter("Username", username),
                        new SqlParameter("Message", message))
                    // A hack to make STORE PROC work when they don't return anything.
                    .AsNoTracking()
                    .TagWith("InsertTweetStoreProc + LogContext")
                    .Select(x => new { })
                    .AsEnumerable()
                    .FirstOrDefault();
            }

            return Task.CompletedTask;
        }
    }


    public interface ITwitterService
    {
        Task<IEnumerable<Tweet>> GetTweets(CancellationToken ct = default);
        Task<IEnumerable<Tweet>> GetTweetsWithQueryTags(CancellationToken ct = default);
        Task<IEnumerable<Tweet>> GetTweetsWithExtraLogs(CancellationToken ct = default);
        Task InsertTweetWithoutLogScope(string username, string message, CancellationToken ct = default);
        Task InsertTweet(string username, string message, CancellationToken ct = default);
        Task InsertTweetStoreProc(string username, string message, CancellationToken ct = default);
    }
}
