using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busfy_api.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_AuthorId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_SubId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_SubscriptionsToAdditionalContent_Subscrip~",
                table: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionsToAdditionalContent");

            migrationBuilder.DropTable(
                name: "UserCreationComments");

            migrationBuilder.DropTable(
                name: "UserCreationLikes");

            migrationBuilder.DropTable(
                name: "UserCreations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "Subscriptions",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "SubId",
                table: "Subscriptions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_AuthorId",
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

            migrationBuilder.AddColumn<string>(
                name: "ContentSubscriptionType",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_CreatorId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_Subscriptions_SubscriptionId",
                table: "UserSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

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
                name: "ContentSubscriptionType",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Subscriptions",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subscriptions",
                newName: "SubId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_CreatorId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                columns: new[] { "SubId", "AuthorId" });

            migrationBuilder.CreateTable(
                name: "SubscriptionsToAdditionalContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
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
                name: "UserCreations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentCategoryName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentSubscriptionType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Filename = table.Column<string>(type: "text", nullable: true),
                    IsFormed = table.Column<bool>(type: "boolean", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCreations_ContentCategories_ContentCategoryName",
                        column: x => x.ContentCategoryName,
                        principalTable: "ContentCategories",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCreations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCreationComments_UserCreations_CreationId",
                        column: x => x.CreationId,
                        principalTable: "UserCreations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCreationComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationLikes",
                columns: table => new
                {
                    EvaluatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationLikes", x => new { x.EvaluatorId, x.CreationId });
                    table.ForeignKey(
                        name: "FK_UserCreationLikes_UserCreations_CreationId",
                        column: x => x.CreationId,
                        principalTable: "UserCreations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCreationLikes_Users_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionsToAdditionalContent_CreatorId",
                table: "SubscriptionsToAdditionalContent",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationComments_CreationId",
                table: "UserCreationComments",
                column: "CreationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationComments_UserId",
                table: "UserCreationComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationLikes_CreationId",
                table: "UserCreationLikes",
                column: "CreationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreations_ContentCategoryName",
                table: "UserCreations",
                column: "ContentCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreations_UserId",
                table: "UserCreations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_AuthorId",
                table: "Subscriptions",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_SubId",
                table: "Subscriptions",
                column: "SubId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_SubscriptionsToAdditionalContent_Subscrip~",
                table: "UserSubscriptions",
                column: "SubscriptionId",
                principalTable: "SubscriptionsToAdditionalContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
