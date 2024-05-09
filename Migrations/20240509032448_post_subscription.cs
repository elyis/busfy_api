using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busfy_api.Migrations
{
    /// <inheritdoc />
    public partial class post_subscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "Posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SubscriptionId",
                table: "Posts",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Subscriptions_SubscriptionId",
                table: "Posts",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Subscriptions_SubscriptionId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_SubscriptionId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Posts");
        }
    }
}
