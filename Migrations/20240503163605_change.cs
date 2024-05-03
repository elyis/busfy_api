using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busfy_api.Migrations
{
    /// <inheritdoc />
    public partial class change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostCreatorId_PostCreationId_PostCategor~",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Posts_PostCreatorId_PostCreationId_PostCategoryNa~",
                table: "PostLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_UserCreations_CreationId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_CreatorId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_Subscriptions_SubscriptionId",
                table: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "UserFavouritePosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreationId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "CountDays",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CreationId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostCreationId",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "PostCreationId",
                table: "PostComments");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Subscriptions",
                newName: "SubscriberId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subscriptions",
                newName: "AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_CreatorId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscriberId");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImage",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentCategoryName",
                table: "UserCreations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SubId",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Posts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Filename",
                table: "Posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommentingAllowed",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFormed",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PostLikes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "ContentCategories",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                columns: new[] { "SubId", "AuthorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                columns: new[] { "CreatorId", "CategoryName" });

            migrationBuilder.CreateTable(
                name: "SelectedUserCategories",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedUserCategories", x => new { x.UserId, x.CategoryName });
                    table.ForeignKey(
                        name: "FK_SelectedUserCategories_ContentCategories_CategoryName",
                        column: x => x.CategoryName,
                        principalTable: "ContentCategories",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SelectedUserCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionsToAdditionalContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CountDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionsToAdditionalContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionsToAdditionalContent_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnconfirmedAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    ValidityPeriodCode = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmationCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnconfirmedAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCreations_ContentCategoryName",
                table: "UserCreations",
                column: "ContentCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AuthorId",
                table: "Subscriptions",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostCreatorId_PostCategoryName",
                table: "PostLikes",
                columns: new[] { "PostCreatorId", "PostCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostCreatorId_PostCategoryName",
                table: "PostComments",
                columns: new[] { "PostCreatorId", "PostCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_SelectedUserCategories_CategoryName",
                table: "SelectedUserCategories",
                column: "CategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionsToAdditionalContent_CreatorId",
                table: "SubscriptionsToAdditionalContent",
                column: "CreatorId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_AuthorId",
                table: "Subscriptions",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_SubscriberId",
                table: "Subscriptions",
                column: "SubscriberId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCreations_ContentCategories_ContentCategoryName",
                table: "UserCreations",
                column: "ContentCategoryName",
                principalTable: "ContentCategories",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_SubscriptionsToAdditionalContent_Subscrip~",
                table: "UserSubscriptions",
                column: "SubscriptionId",
                principalTable: "SubscriptionsToAdditionalContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostCreatorId_PostCategoryName",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Posts_PostCreatorId_PostCategoryName",
                table: "PostLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_AuthorId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_SubscriberId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCreations_ContentCategories_ContentCategoryName",
                table: "UserCreations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_SubscriptionsToAdditionalContent_Subscrip~",
                table: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "SelectedUserCategories");

            migrationBuilder.DropTable(
                name: "SubscriptionsToAdditionalContent");

            migrationBuilder.DropTable(
                name: "UnconfirmedAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UserCreations_ContentCategoryName",
                table: "UserCreations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_AuthorId",
                table: "Subscriptions");

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
                name: "BackgroundImage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentCategoryName",
                table: "UserCreations");

            migrationBuilder.DropColumn(
                name: "SubId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Filename",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsCommentingAllowed",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsFormed",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "ContentCategories");

            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "Subscriptions",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "Subscriptions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_SubscriberId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_CreatorId");

            migrationBuilder.AddColumn<int>(
                name: "CountDays",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<float>(
                name: "Price",
                table: "Subscriptions",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Subscriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreationId",
                table: "Posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreationId",
                table: "PostLikes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostCreationId",
                table: "PostComments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                columns: new[] { "CreatorId", "CreationId", "CategoryName" });

            migrationBuilder.CreateTable(
                name: "UserFavouritePosts",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostCreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostCreationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostCategoryName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavouritePosts", x => new { x.UserId, x.PostId });
                    table.ForeignKey(
                        name: "FK_UserFavouritePosts_Posts_PostCreatorId_PostCreationId_PostC~",
                        columns: x => new { x.PostCreatorId, x.PostCreationId, x.PostCategoryName },
                        principalTable: "Posts",
                        principalColumns: new[] { "CreatorId", "CreationId", "CategoryName" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavouritePosts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreationId",
                table: "Posts",
                column: "CreationId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostLikes",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostCreatorId_PostCreationId_PostCategoryName",
                table: "PostComments",
                columns: new[] { "PostCreatorId", "PostCreationId", "PostCategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouritePosts_PostCreatorId_PostCreationId_PostCategor~",
                table: "UserFavouritePosts",
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
                name: "FK_Posts_UserCreations_CreationId",
                table: "Posts",
                column: "CreationId",
                principalTable: "UserCreations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_CreatorId",
                table: "Subscriptions",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_Subscriptions_SubscriptionId",
                table: "UserSubscriptions",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
