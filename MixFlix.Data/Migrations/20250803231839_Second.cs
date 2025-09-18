using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MixFlix.Data.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "dislike_count",
                table: "content",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "imdb_score",
                table: "content",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "imdb_votes",
                table: "content",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "just_watch_rating",
                table: "content",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "like_count",
                table: "content",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "tmdb_popularity",
                table: "content",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "tmdb_score",
                table: "content",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tomato_meter",
                table: "content",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "content_availability",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_seen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_availability", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_availability_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_content_availability_services_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_content_availability_content_id",
                table: "content_availability",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_availability_service_id",
                table: "content_availability",
                column: "service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_availability");

            migrationBuilder.DropColumn(
                name: "dislike_count",
                table: "content");

            migrationBuilder.DropColumn(
                name: "imdb_score",
                table: "content");

            migrationBuilder.DropColumn(
                name: "imdb_votes",
                table: "content");

            migrationBuilder.DropColumn(
                name: "just_watch_rating",
                table: "content");

            migrationBuilder.DropColumn(
                name: "like_count",
                table: "content");

            migrationBuilder.DropColumn(
                name: "tmdb_popularity",
                table: "content");

            migrationBuilder.DropColumn(
                name: "tmdb_score",
                table: "content");

            migrationBuilder.DropColumn(
                name: "tomato_meter",
                table: "content");
        }
    }
}
