using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busfy_api.Migrations
{
    /// <inheritdoc />
    public partial class add_category_to_post : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostId",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Posts_PostId",
                table: "PostLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavouritePosts_Posts_PostId",
                table: "UserFavouritePosts");

            migrationBuilder.DropIndex(
                name: "IX_UserFavouritePosts_PostId",
                table: "UserFavouritePosts");

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

            migrationBuilder.AddColumn<string>(
                name: "PostCategoryName",
                table: "UserFavouritePosts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreationId",
                table: "UserFavouritePosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreatorId",
                table: "UserFavouritePosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostCategoryName",
                table: "PostLikes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreationId",
                table: "PostLikes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                name: "PostCreationId",
                table: "PostComments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreatorId",
                table: "PostComments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                columns: new[] { "CreatorId", "CreationId", "CategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouritePosts_PostCreatorId_PostCreationId_PostCategor~",
                table: "UserFavouritePosts",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CategoryName",
                table: "Posts",
                column: "CategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostLikes",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostComments",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" });

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostCreatorId_PostCreationId_PostCategor~",
                table: "PostComments",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" },
                principalTable: "Posts",
                principalColumns: new[] { "CreatorId", "CreationId", "CategoryName" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLikes_Posts_PostCreatorId_PostCreationId_PostCategoryNa~",
                table: "PostLikes",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" },
                principalTable: "Posts",
                principalColumns: new[] { "CreatorId", "CreationId", "CategoryName" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_ContentCategories_CategoryName",
                table: "Posts",
                column: "CategoryName",
                principalTable: "ContentCategories",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavouritePosts_Posts_PostCreatorId_PostCreationId_PostC~",
                table: "UserFavouritePosts",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" },
                principalTable: "Posts",
                principalColumns: new[] { "CreatorId", "CreationId", "CategoryName" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostCreatorId_PostCreationId_PostCategor~",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Posts_PostCreatorId_PostCreationId_PostCategoryNa~",
                table: "PostLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_ContentCategories_CategoryName",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavouritePosts_Posts_PostCreatorId_PostCreationId_PostC~",
                table: "UserFavouritePosts");

            migrationBuilder.DropIndex(
                name: "IX_UserFavouritePosts_PostCreatorId_PostCreationId_PostCategor~",
                table: "UserFavouritePosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CategoryName",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostCategoryName",
                table: "UserFavouritePosts");

            migrationBuilder.DropColumn(
                name: "PostCreationId",
                table: "UserFavouritePosts");

            migrationBuilder.DropColumn(
                name: "PostCreatorId",
                table: "UserFavouritePosts");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostCategoryName",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "PostCreationId",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "PostCreatorId",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "PostCategoryName",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostCreationId",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostCreatorId",
                table: "PostComments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouritePosts_PostId",
                table: "UserFavouritePosts",
                column: "PostId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavouritePosts_Posts_PostId",
                table: "UserFavouritePosts",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
