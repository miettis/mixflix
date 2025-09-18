using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixFlix.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContentTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content_availability_contents_content_id",
                table: "content_availability");

            migrationBuilder.DropForeignKey(
                name: "fk_content_availability_services_service_id",
                table: "content_availability");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "tmdb_last_seen",
                table: "content_availability",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "just_watch_last_seen",
                table: "content_availability",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created",
                table: "content",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "modified",
                table: "content",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_content_availability_content_content_id",
                table: "content_availability",
                column: "content_id",
                principalTable: "content",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_availability_service_service_id",
                table: "content_availability",
                column: "service_id",
                principalTable: "service",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content_availability_content_content_id",
                table: "content_availability");

            migrationBuilder.DropForeignKey(
                name: "fk_content_availability_service_service_id",
                table: "content_availability");

            migrationBuilder.DropColumn(
                name: "created",
                table: "content");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "content");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "tmdb_last_seen",
                table: "content_availability",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "just_watch_last_seen",
                table: "content_availability",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_content_availability_contents_content_id",
                table: "content_availability",
                column: "content_id",
                principalTable: "content",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_content_availability_services_service_id",
                table: "content_availability",
                column: "service_id",
                principalTable: "service",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
