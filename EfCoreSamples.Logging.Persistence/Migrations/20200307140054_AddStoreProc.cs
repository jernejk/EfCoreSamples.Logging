using Microsoft.EntityFrameworkCore.Migrations;

namespace EfCoreSamples.Logging.Persistence.Migrations
{
    public partial class AddStoreProc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- =============================================
-- Author:              JK
-- Create date: 2020/02/18
-- Description: Just for demonstrating logging
-- =============================================
CREATE PROCEDURE InsertTweet
        @Username nvarchar,
        @Message nvarchar
AS
BEGIN
        INSERT INTO Tweets
           (Id, Username, Message, CreatedUtc)
     VALUES
           (NEWID(), @Username, @Message, GETUTCDATE())
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE InsertTweet");
        }
    }
}
