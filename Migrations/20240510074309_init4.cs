using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busfy_api.Migrations
{
    /// <inheritdoc />
    public partial class init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockDuration",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockReason",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "FavoritePosts",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritePosts", x => new { x.PostId, x.UserId });
                    table.ForeignKey(
                        name: "FK_FavoritePosts_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritePosts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePosts_UserId",
                table: "FavoritePosts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoritePosts");

            migrationBuilder.AddColumn<DateTime>(
                name: "LockDuration",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockReason",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
