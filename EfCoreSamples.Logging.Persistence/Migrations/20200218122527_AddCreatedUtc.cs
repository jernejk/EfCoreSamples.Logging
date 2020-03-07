using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EfCoreSamples.Logging.Persistence.Migrations
{
    public partial class AddCreatedUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Tweets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Tweets");
        }
    }
}
