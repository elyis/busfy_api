using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace busfy_api.Migrations
{
    /// <inheritdoc />
    public partial class init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_SubscriptionId",
                table: "Posts");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SubscriptionId",
                table: "Posts",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_SubscriptionId",
                table: "Posts");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SubscriptionId",
                table: "Posts",
                column: "SubscriptionId",
                unique: true);
        }
    }
}
