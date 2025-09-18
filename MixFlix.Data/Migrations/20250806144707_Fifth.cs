using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixFlix.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fifth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content_categories_category_category_id",
                table: "content_categories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_group_members",
                table: "group_members");

            migrationBuilder.AddColumn<string>(
                name: "logo",
                table: "service",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "group_members",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "group_members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "require_approval",
                table: "group",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_group_members",
                table: "group_members",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_group_members_group_id",
                table: "group_members",
                column: "group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_content_categories_categories_category_id",
                table: "content_categories",
                column: "category_id",
                principalTable: "category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_content_categories_categories_category_id",
                table: "content_categories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_group_members",
                table: "group_members");

            migrationBuilder.DropIndex(
                name: "ix_group_members_group_id",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "logo",
                table: "service");

            migrationBuilder.DropColumn(
                name: "id",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "status",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "require_approval",
                table: "group");

            migrationBuilder.AddPrimaryKey(
                name: "pk_group_members",
                table: "group_members",
                columns: new[] { "group_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_content_categories_category_category_id",
                table: "content_categories",
                column: "category_id",
                principalTable: "category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
