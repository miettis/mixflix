using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixFlix.Data.Migrations
{
    /// <inheritdoc />
    public partial class Seventh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cast",
                table: "content",
                type: "text",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "imdb_id",
                table: "content",
                type: "text", 
                maxLength: 20,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "languages",
                table: "content",
                type: "text", 
                maxLength: 20,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tmdb_id",
                table: "content",
                type: "text", 
                maxLength: 20,
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cast",
                table: "content");

            migrationBuilder.DropColumn(
                name: "imdb_id",
                table: "content");

            migrationBuilder.DropColumn(
                name: "languages",
                table: "content");

            migrationBuilder.DropColumn(
                name: "tmdb_id",
                table: "content");
        }
    }
}
