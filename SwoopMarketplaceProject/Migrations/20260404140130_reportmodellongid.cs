using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwoopMarketplaceProject.Migrations
{
    /// <inheritdoc />
    public partial class reportmodellongid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reports_listings_ListingId1",
                table: "reports");

            migrationBuilder.DropForeignKey(
                name: "FK_reports_users_UserId1",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_ListingId1",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_UserId1",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "ListingId1",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "reports");

            migrationBuilder.AlterColumn<string>(
                name: "condition",
                table: "listings",
                type: "enum('fn','mw','ft','ww','bs')",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "enum('new','used')")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_reports_listingId",
                table: "reports",
                column: "listingId");

            migrationBuilder.CreateIndex(
                name: "IX_reports_userId",
                table: "reports",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_reports_listings_listingId",
                table: "reports",
                column: "listingId",
                principalTable: "listings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_users_userId",
                table: "reports",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reports_listings_listingId",
                table: "reports");

            migrationBuilder.DropForeignKey(
                name: "FK_reports_users_userId",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_listingId",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_userId",
                table: "reports");

            migrationBuilder.AddColumn<long>(
                name: "ListingId1",
                table: "reports",
                type: "bigint(20)",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "reports",
                type: "bigint(20)",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "condition",
                table: "listings",
                type: "enum('new','used')",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "enum('fn','mw','ft','ww','bs')")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_reports_ListingId1",
                table: "reports",
                column: "ListingId1");

            migrationBuilder.CreateIndex(
                name: "IX_reports_UserId1",
                table: "reports",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_reports_listings_ListingId1",
                table: "reports",
                column: "ListingId1",
                principalTable: "listings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_users_UserId1",
                table: "reports",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
