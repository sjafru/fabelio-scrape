using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FabelioScrape.Web.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PageUrl = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(maxLength: 512, nullable: true),
                    Title = table.Column<string>(maxLength: 64, nullable: true),
                    SubTitle = table.Column<string>(maxLength: 64, nullable: true),
                    FinalPrice = table.Column<int>(nullable: false),
                    OldPrice = table.Column<int>(nullable: false),
                    ImageUrls_Json = table.Column<string>(maxLength: 1000, nullable: true),
                    LastSyncAt = table.Column<DateTimeOffset>(nullable: false),
                    LastSyncStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
