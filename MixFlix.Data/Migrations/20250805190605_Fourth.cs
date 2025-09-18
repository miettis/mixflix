using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixFlix.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fourth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_users_creator_id",
                table: "groups");

            migrationBuilder.DropForeignKey(
                name: "fk_user_groups_group_id",
                table: "user");

            migrationBuilder.DropIndex(
                name: "ix_user_group_id",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_groups",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "user");

            migrationBuilder.RenameTable(
                name: "groups",
                newName: "group");

            migrationBuilder.RenameIndex(
                name: "ix_groups_creator_id",
                table: "group",
                newName: "ix_group_creator_id");

            migrationBuilder.AlterColumn<int>(
                name: "ranking",
                table: "content_availability",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "pk_group",
                table: "group",
                column: "id");

            migrationBuilder.CreateTable(
                name: "group_members",
                columns: table => new
                {
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_members", x => new { x.group_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_group_members_group_group_id",
                        column: x => x.group_id,
                        principalTable: "group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_members_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_rating",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_rating", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_rating_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_rating_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_group_members_user_id",
                table: "group_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_rating_content_id",
                table: "user_rating",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_rating_user_id",
                table: "user_rating",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_group_user_creator_id",
                table: "group",
                column: "creator_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_group_user_creator_id",
                table: "group");

            migrationBuilder.DropTable(
                name: "group_members");

            migrationBuilder.DropTable(
                name: "user_rating");

            migrationBuilder.DropPrimaryKey(
                name: "pk_group",
                table: "group");

            migrationBuilder.RenameTable(
                name: "group",
                newName: "groups");

            migrationBuilder.RenameIndex(
                name: "ix_group_creator_id",
                table: "groups",
                newName: "ix_groups_creator_id");

            migrationBuilder.AddColumn<Guid>(
                name: "group_id",
                table: "user",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ranking",
                table: "content_availability",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_groups",
                table: "groups",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_user_group_id",
                table: "user",
                column: "group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_groups_users_creator_id",
                table: "groups",
                column: "creator_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_groups_group_id",
                table: "user",
                column: "group_id",
                principalTable: "groups",
                principalColumn: "id");
        }
    }
}
