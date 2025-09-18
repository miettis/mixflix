using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixFlix.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExternalToJustWatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "external_id",
                table: "content",
                newName: "just_watch_id");

            migrationBuilder.RenameColumn(
                name: "languages",
                table: "content",
                newName: "language");

            migrationBuilder.RenameColumn(
                name: "external_id",
                table: "service",
                newName: "just_watch_id");

            migrationBuilder.RenameColumn(
                name: "ranking",
                table: "content_availability",
                newName: "just_watch_ranking");

            migrationBuilder.RenameColumn(
                name: "last_seen",
                table: "content_availability",
                newName: "just_watch_last_seen");

            migrationBuilder.RenameColumn(
                name: "external_id",
                table: "category",
                newName: "just_watch_id");

            migrationBuilder.AddColumn<string>(
                name: "tmdb_id",
                table: "service",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "title_en",
                table: "content",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "imdb_id",
                table: "content",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "cast",
                table: "content",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "tmdb_id",
                table: "category",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "just_watch_id",
                table: "service");

            migrationBuilder.DropColumn(
                name: "just_watch_id",
                table: "content");

            migrationBuilder.DropColumn(
                name: "language",
                table: "content");

            migrationBuilder.DropColumn(
                name: "just_watch_id",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "tmdb_id",
                table: "service",
                newName: "external_id");

            migrationBuilder.RenameColumn(
                name: "just_watch_ranking",
                table: "content_availability",
                newName: "ranking");

            migrationBuilder.RenameColumn(
                name: "just_watch_last_seen",
                table: "content_availability",
                newName: "last_seen");

            migrationBuilder.RenameColumn(
                name: "tmdb_id",
                table: "category",
                newName: "external_id");

            migrationBuilder.AlterColumn<string>(
                name: "tmdb_id",
                table: "content",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title_en",
                table: "content",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "imdb_id",
                table: "content",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cast",
                table: "content",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "content",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "languages",
                table: "content",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
