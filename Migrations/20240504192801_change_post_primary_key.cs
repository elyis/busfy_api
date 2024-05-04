using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busfy_api.Migrations
{
    /// <inheritdoc />
    public partial class change_post_primary_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostCreatorId_PostCategoryName",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Posts_PostCreatorId_PostCategoryName",
                table: "PostLikes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_PostCreatorId_PostCategoryName",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_PostCreatorId_PostCategoryName",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostCategoryName",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "PostCreatorId",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "PostCategoryName",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostCreatorId",
                table: "PostComments");

            migrationBuilder.AddColumn<bool>(
                name: "IsFormed",
                table: "UserCreations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "UserCreations",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostId",
                table: "PostLikes",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostId",
                table: "PostComments",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostId",
                table: "PostComments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLikes_Posts_PostId",
                table: "PostLikes",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostId",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Posts_PostId",
                table: "PostLikes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_PostId",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_PostId",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "IsFormed",
                table: "UserCreations");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "UserCreations");

            migrationBuilder.AddColumn<string>(
                name: "PostCategoryName",
                table: "PostLikes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreatorId",
                table: "PostLikes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PostCategoryName",
                table: "PostComments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreatorId",
                table: "PostComments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                columns: new[] { "CreatorId", "CategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostCreatorId_PostCategoryName",
                table: "PostLikes",
                columns: new[] { "PostCreatorId", "PostCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostCreatorId_PostCategoryName",
                table: "PostComments",
                columns: new[] { "PostCreatorId", "PostCategoryName" });

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostCreatorId_PostCategoryName",
                table: "PostComments",
                columns: new[] { "PostCreatorId", "PostCategoryName" },
                principalTable: "Posts",
                principalColumns: new[] { "CreatorId", "CategoryName" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLikes_Posts_PostCreatorId_PostCategoryName",
                table: "PostLikes",
                columns: new[] { "PostCreatorId", "PostCategoryName" },
                principalTable: "Posts",
                principalColumns: new[] { "CreatorId", "CategoryName" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
