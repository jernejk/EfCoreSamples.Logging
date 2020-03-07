using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EfCoreSamples.Logging.Persistence.Migrations
{
    public partial class SeedTweets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tweets",
                columns: new[] { "Id", "Username", "Message", "CreatedUtc" },
                values: new object[] { Guid.NewGuid(), "jernej_kavka", "Logs are awesome!", new DateTime(2020, 2, 18, 20, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Tweets",
                columns: new[] { "Id", "Username", "Message", "CreatedUtc" },
                values: new object[] { Guid.NewGuid(), "jernej_kavka", "EF Core has nice support logging, however it's hard to get from SQL query back to original code", new DateTime(2020, 2, 18, 20, 30, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Tweets",
                columns: new[] { "Id", "Username", "Message", "CreatedUtc" },
                values: new object[] { Guid.NewGuid(), "jernej_kavka", "This sample app shows you how to track EF Core queries with `IQueryable<T>.TagWith()` and `ILogger<T>.BeginScope()`.", new DateTime(2020, 2, 18, 21, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Tweets",
                columns: new[] { "Id", "Username", "Message", "CreatedUtc" },
                values: new object[] { Guid.NewGuid(), "jernej_kavka", "You'll be able to see rich logs in Seq and/or in Application Insights.", new DateTime(2020, 2, 18, 23, 0, 0, DateTimeKind.Utc) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
