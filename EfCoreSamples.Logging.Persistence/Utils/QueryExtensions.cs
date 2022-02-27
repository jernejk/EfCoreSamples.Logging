using Microsoft.Extensions.Logging;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore;

public static class QueryExtensions
{
    /// <summary>
    /// This is an alternative to `.WithTag(comment)`.
    /// 
    /// You can use it like `.TagWithContext()` and this will auto-generated comment based on the class and method name ("Class-Method").
    /// Or you can add additional comment to it with `.TagWithContext("CheckAny")` (eg. "TweeterService-DeleteTweet-CheckAny")
    /// </summary>
    public static IQueryable<T> TagWithContext<T>(this IQueryable<T> queryable, string message = "", [CallerFilePath] string callerFileName = "", [CallerMemberName] string callerName = "")
    {
        string logScopeName = LoggingExtensions.GenerateLogScopeName(message, callerFileName, callerName);
        return queryable.TagWith(logScopeName);
    }

    /// <summary>
    /// Alternative to `.TagWith` which allows you to add 2 comments together into 1.
    /// </summary>
    public static IQueryable<T> TagWith<T>(this IQueryable<T> queryable, string logScopeName, string message)
    {
        return queryable.TagWith($"{logScopeName}-{message}");
    }
}
